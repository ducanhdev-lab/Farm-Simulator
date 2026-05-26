using System;
using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    [CreateAssetMenu(fileName = "Recipe", menuName = "Island Harvest/Recipe")]
    public class RecipeData : ScriptableObject
    {
        [SerializeField] private string recipeId;
        [SerializeField] private float processDuration = 2f;
        [SerializeField] private List<RecipeIngredient> inputs = new List<RecipeIngredient>();
        [SerializeField] private RecipeIngredient output;

        public string RecipeId => recipeId;
        public float ProcessDuration => processDuration;
        public IReadOnlyList<RecipeIngredient> Inputs => inputs;
        public RecipeIngredient Output => output;
    }

    [Serializable]
    public struct RecipeIngredient
    {
        public ItemData item;
        public int amount;
    }
}
