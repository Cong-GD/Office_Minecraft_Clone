using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Minecraft
{

    public class RelativeRecipes
    {
        public BaseItem_SO item;
        public List<Recipe_SO> recipes;
    }

    public class UIRecipeDictionary : MonoBehaviour
    {
        [SerializeField]
        private Canvas myCanvas;

        [SerializeField]
        private CreavityUIItemSlot[] craftingSlot;

        [SerializeField]
        private CreavityUIItemSlot resultSlot;

        [SerializeField]
        private MinecraftButton leftButton;

        [SerializeField]
        private MinecraftButton rightButton;

        [SerializeField]
        private TextMeshProUGUI currentIndexText;

        [SerializeField]
        private TMP_InputField searchPatternText;

        private RelativeRecipes[] _relativeRecipes;

        private Recipe_SO[] _displayRecipes = Array.Empty<Recipe_SO>();

        private int _currentSelected = 0;
        private Coroutine _displayingCoroutine; 
        
        private void OnEnable()
        {
           myCanvas.enabled = true;
        }

        private void OnDisable()
        {
            myCanvas.enabled = false;
        }

        private async void Start()
        {
            Recipe_SO[] recipes = Resources.LoadAll<Recipe_SO>("Recipes");
            await UniTask.SwitchToThreadPool();
            List<RelativeRecipes> relativeRecipes = new List<RelativeRecipes>();
            foreach (Recipe_SO recipe in recipes)
            {
                foreach (BaseItem_SO item in recipe.GetRelativeItems())
                {
                    int index = relativeRecipes.FindIndex(relativeRecipe => relativeRecipe.item == item);
                    if(index == -1)
                    {
                        relativeRecipes.Add(new RelativeRecipes
                        {
                            item = item,
                            recipes = new List<Recipe_SO> { recipe }
                        });
                    }
                    else
                    {
                        if (!relativeRecipes[index].recipes.Contains(recipe))
                            relativeRecipes[index].recipes.Add(recipe);
                    }
                }
            }
            _relativeRecipes = relativeRecipes.ToArray();
            await UniTask.SwitchToMainThread();
            DisplayRecipe("");
        }
        public void DisplayRecipe(string searchPattern)
        {
            GetDisplayRecipse(searchPattern);
            _currentSelected = 0;
            UpdateButtonInteractable();
            UpdateText();
            UpdateRecipeSlot();
        }

        public void OnLeftButtonClicked()
        {
            _currentSelected--;
            _currentSelected = Mathf.Max(_currentSelected, 0);
            UpdateButtonInteractable();
            UpdateText();
            UpdateRecipeSlot();
        }

        public void OnRightButtonClicked()
        {
            _currentSelected++;
            _currentSelected = Mathf.Min(_currentSelected, _displayRecipes.Length - 1);
            UpdateButtonInteractable();
            UpdateText();
            UpdateRecipeSlot();
        }

        public void OnItemSlotClick(PointerEventData eventData, CreavityUIItemSlot slot)
        {
            if (slot.Item == null)
                return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                searchPatternText.text = slot.Item.Name;
            }
        }

        private void GetDisplayRecipse(string searchPattern)
        {
            const StringComparison ignoreCase = StringComparison.CurrentCultureIgnoreCase;

            _displayRecipes = _relativeRecipes
                .Where(relative => relative.item.Name.Contains(searchPattern, ignoreCase))
                .SelectMany(relative => relative.recipes)
                .Distinct()
                .ToArray();
        }   

        private void UpdateButtonInteractable()
        {
            if(_displayRecipes.Length <= 1)
            {
                leftButton.SetInteractable(false);
                rightButton.SetInteractable(false);
            }
            else
            {
                leftButton.SetInteractable(_currentSelected > 0);
                rightButton.SetInteractable(_currentSelected < _displayRecipes.Length - 1);
            }
        }
        private void UpdateText()
        {             
            if(_displayRecipes.Length == 0)
            {
                currentIndexText.text = "0/0";
            }
            else
            {
                currentIndexText.text = $"{_currentSelected + 1}/{_displayRecipes.Length}";
            }
        }

        private void UpdateRecipeSlot()
        {
            if(_displayRecipes.Length == 0)
            {
                resultSlot.SetItem(null);
                for (int i = 0; i < craftingSlot.Length; i++)
                {
                    craftingSlot[i].SetItem(null);
                }
            }
            else
            {
                Recipe_SO recipe = _displayRecipes[_currentSelected];
                resultSlot.SetItem(recipe.GetResult().item);
                for (int i = 0; i < craftingSlot.Length; i++)
                {
                    craftingSlot[i].SetItem(recipe.GetItem(i % 3, i / 3));
                }
            }
        }
    }
}
