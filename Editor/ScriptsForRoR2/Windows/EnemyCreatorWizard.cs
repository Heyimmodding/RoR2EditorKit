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
        public int creditCost;

        public EquipmentDef[] chosenEquipment;
        
        [Serializable]
        public struct itemsToGrant
        {
            public ItemDef itemDef;
            public int itemCount;
        }

        public List<itemsToGrant> chosenItems = new List<itemsToGrant>();

        public Dictionary<ItemDef, int> ItemsToGrant()
        {
            return chosenItems.ToDictionary(k => k.itemDef, v => v.itemCount);
        }

        [CustomPropertyDrawer(typeof(itemsToGrant))]
        public class ItemPropertyDrawer : PropertyDrawer
        {
            public override VisualElement CreatePropertyGUI(SerializedProperty property)
            {
                var container = new VisualElement();

                var itemNameProperty = new PropertyField(property.FindPropertyRelative("itemDef"));
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
        private GameObject instantiatedBody;

        protected override async Task<bool> RunWizard()
        {
           
            if (characterBody = null)
            {
                Debug.LogError("No CharacterBody found!");
                return false;
            }

            Debug.LogError(characterBody);

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

            ItemCountPair[] bodyItems = new ItemCountPair[0];
            foreach(ItemDef def in ItemsToGrant().Keys)
            {
                ItemCountPair temp = new ItemCountPair
                {
                    itemDef = def,
                    count = ItemsToGrant()[def]
                };

                bodyItems.Append(temp);
            }
           
            if(bodyItems.Length > 0)
            {
                csc.itemsToGrant = bodyItems;
            }
            if(chosenEquipment.Length > 0)
            {
                csc.equipmentToGrant = chosenEquipment;
            }
            

            var directory = IOUtils.GetCurrentDirectory();
            var destpath = FileUtil.GetProjectRelativePath(IOUtils.FormatPathForUnity(Path.Combine(directory, $"csc{bodyName}.asset")));

            AssetDatabase.CreateAsset(csc, destpath);
            return Task.CompletedTask;
        }

       
        
    }

}
