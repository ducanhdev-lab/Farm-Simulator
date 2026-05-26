using System.Collections;
using UnityEngine;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Converts inputs to outputs using a RecipeData. Place on Home Island near a silo/stall.
    /// </summary>
    public class ProcessingStation : MonoBehaviour
    {
        [SerializeField] private RecipeData recipe;
        [SerializeField] private Inventory inputInventory;
        [SerializeField] private Inventory outputInventory;
        [SerializeField] private Transform processIndicator;

        private bool isProcessing;

        private void Reset()
        {
            inputInventory = GetComponent<Inventory>();
        }

        private void OnTriggerStay(Collider other)
        {
            if (isProcessing || recipe == null || !other.CompareTag("Player"))
                return;

            if (!HasIngredients(inputInventory ?? other.GetComponent<PlayerController>()?.Inventory))
                return;

            StartCoroutine(ProcessRoutine(other.GetComponent<PlayerController>()?.Inventory));
        }

        private bool HasIngredients(Inventory source)
        {
            if (source == null)
                return false;

            foreach (var ingredient in recipe.Inputs)
            {
                if (ingredient.item == null)
                    continue;

                var stack = source.Items.Find(i => i.ItemId == ingredient.item.ItemId);
                if (stack == null || stack.Amount < ingredient.amount)
                    return false;
            }

            return true;
        }

        private IEnumerator ProcessRoutine(Inventory playerInventory)
        {
            var source = inputInventory != null ? inputInventory : playerInventory;
            if (source == null)
                yield break;

            isProcessing = true;
            if (processIndicator != null)
                processIndicator.gameObject.SetActive(true);

            yield return new WaitForSeconds(recipe.ProcessDuration);

            foreach (var ingredient in recipe.Inputs)
            {
                if (ingredient.item != null)
                    source.SubtractItem(ingredient.item.ItemId, ingredient.amount);
            }

            var target = outputInventory != null ? outputInventory : playerInventory;
            if (recipe.Output.item != null && target != null)
            {
                target.AddItem(recipe.Output.item.ItemId, recipe.Output.amount);
                GameEventBus.RaiseItemProcessed(recipe.RecipeId, recipe.Output.amount);
            }

            if (processIndicator != null)
                processIndicator.gameObject.SetActive(false);

            isProcessing = false;
        }
    }
}
