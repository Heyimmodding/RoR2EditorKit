using HG;
using RoR2;
using RoR2EditorKit.Common;
using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using Path = System.IO.Path;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class EnemyCreatorWizardWindow : CreatorWizardWindow 
    {
        public CharacterBody characterBody;
        public bool isFlying;
        public bool noElites;
        public bool lunarEnemy;
        public bool bossForbidden;
        public int? creditCost;
        public string chosenEquipment;
        
        [Serializable]
        public struct itemsToGrant
        {
            public string itemName;
            public int itemCount;
        }

        public List<itemsToGrant> chosenItems = new List<itemsToGrant>();

        public Dictionary<string, int> ItemsToGrant()
        {
            return chosenItems.ToDictionary(k => k.itemName, v => v.itemCount);
        }

        [CustomPropertyDrawer(typeof(itemsToGrant))]
        public class ItemPropertyDrawer : PropertyDrawer
        {
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                var container = new VisualElement();

                var itemNameProperty = new PropertyField(property.FindPropertyRelative("itemName"));
                var itemCountProperty = new PropertyField(property.FindPropertyRelative("itemCount"));

                container.Add(itemNameProperty);
                container.Add(itemCountProperty);
                return container;

            }
        }

        private string bodyName;
        
        protected override string WizardTitleTooltip => base.WizardTitleTooltip;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/Enemy", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<EnemyCreatorWizardWindow>();
            window.Focus();
        }

        private GameObject tempPrefab;
        private GameObject finishedMaster;

        protected override async Task<bool> RunWizard()
        {
            if(characterBody.gameObject.name.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("Prefab name is null, empty, or whitespace! Is this intended?");
                return false;

            }
            if (characterBody = null)
            {
                Debug.LogError("No CharacterBody found!");
                return false;
            }
            if (!creditCost.HasValue)
            {
                Debug.LogError("Please assign a credit value!");
                return false;
            }
            bodyName = characterBody.gameObject.name.Replace("Body", string.Empty);

            try
            {
                await InstantiateTemplate();
                await FillPrefabFields();
                await CreatePrefab();
                await CreateCSC();
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
            return true;
        }

        protected override void Cleanup()
        {
            DestroyImmediate(tempPrefab);
        }
        private Task InstantiateTemplate()
        {
            var prefab = Constants.AssetGUIDS.QuickLoad<GameObject>(Constants.AssetGUIDS.characterMasterTemplate);
            tempPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            return Task.CompletedTask;
        }

        private Task FillPrefabFields()
        {
            CharacterMaster master = tempPrefab.GetComponent<CharacterMaster>();
            master.bodyPrefab = characterBody.gameObject;
            tempPrefab.name = $"{bodyName}MonsterMaster";
            return Task.CompletedTask;
        }

        private Task CreatePrefab()
        {
            var path = IOUtils.GetCurrentDirectory();
            var destpath = IOUtils.FormatPathForUnity(Path.Combine(path, tempPrefab.name + ".prefab"));
            var projectrelative = FileUtil.GetProjectRelativePath(destpath);
            finishedMaster = PrefabUtility.SaveAsPrefabAsset(tempPrefab, projectrelative);
            AssetDatabase.ImportAsset(projectrelative);
            return Task.CompletedTask;
        }

        private Task CreateCSC()
        {
            var csc = ScriptableObject.CreateInstance<CharacterSpawnCard>();

            csc.prefab = finishedMaster;
            csc.sendOverNetwork = true;
            csc.hullSize = characterBody.hullClassification;
            if (isFlying)
            {
                csc.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Air;
            }
            else
            {
                csc.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
            }
            csc.noElites = noElites;
            csc.forbiddenAsBoss = bossForbidden;
            csc.directorCreditCost = (int)creditCost;
            csc.forbiddenFlags = RoR2.Navigation.NodeFlags.NoCharacterSpawn;
            if (lunarEnemy)
            {
                csc.eliteRules = SpawnCard.EliteRules.Lunar;
            }
            else
            {
                csc.eliteRules = SpawnCard.EliteRules.Default;
            }
            
            //compiles a list of every itemdef in the game (does not work with modded items)
            FieldInfo[] itemList = typeof(RoR2Content.Items).GetFields().Concat(typeof(DLC1Content.Items).GetFields()).ToArray();
            //compiles a list of every equipmentdef in the game (again, doesnt work with mods. the idea is that you can just manually input your item/equip def that you made. However, for other mods the swap must be done at runtime)
            FieldInfo[] equipList = typeof(RoR2Content.Equipment).GetFields().Concat(typeof(DLC1Content.Equipment).GetFields()).ToArray();

            List<ItemDef> fetchedItems = new List<ItemDef>();

            //Checks the list of every item def to find the chosen items and formats them to be uniform for later
            foreach (string itemDefToFind in ItemsToGrant().Keys)
            {
                for (int i = 0; i < itemList.Length; i++)
                {
                    if (itemList.GetValue(i).ToString().ToLower() == itemDefToFind.ToLower())
                    {
                        fetchedItems.Add((ItemDef)itemList.GetValue(i));
                    }
                }
                ItemsToGrant().Remove(itemDefToFind);
                ItemsToGrant().Add(itemDefToFind.ToLower(), ItemsToGrant()[itemDefToFind]);
            }

            //creates an array to contain our pairs
            ItemCountPair[] bodyItems = new ItemCountPair[0];
            //creates a local equipdef to assign to the body
            EquipmentDef[] bodyEquip = new EquipmentDef[0];

            //creates an itemcountpair for each chosen item
            foreach (ItemDef itemDef in fetchedItems)
            {
                string[] formatName = itemDef.nameToken.Split('_');
                string itemName = formatName[2].ToLower();

                ItemCountPair temp = new ItemCountPair
                {
                    itemDef = itemDef,
                    count = ItemsToGrant()[itemName]
                };

                bodyItems.Append(temp);
            }

            for(int i = 0; i < equipList.Length; i++)
            {
                if(equipList.GetValue(i).ToString().ToLower() == chosenEquipment.ToLower())
                {
                    bodyEquip.Append((EquipmentDef)equipList.GetValue(i));
                }
            }

            csc.itemsToGrant = bodyItems;
            csc.equipmentToGrant = bodyEquip;

            var directory = IOUtils.GetCurrentDirectory();
            var destpath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(Path.Combine(directory, $"csc{bodyName}.asset")));

            AssetDatabase.CreateAsset(csc, destpath);
            return Task.CompletedTask;
        }

       
        
    }

}
