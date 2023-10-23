using UnityEngine;

public class Inventory : MonoBehaviour
{
    public readonly ItemSlot[] inventory = ItemUtility.NewStogare(27);

    public readonly ItemSlot[] toolBarItems = ItemUtility.NewStogare(9);


    [SerializeField] private ItemFactory[] itemFactory;

    public ItemSlot HandItem {  get; private set; }

    public ItemSlot OffHand {  get; private set; } = new ItemSlot();

    private void Start()
    {
        for (int i = 0; i < itemFactory.Length && i < toolBarItems.Length; i++) 
        {
            var createdItem = itemFactory[i].Create();
            createdItem.TransferTo(toolBarItems[i], createdItem.Amount);
        }
    }

    public void SetHand(ItemSlot hand)
    {
        HandItem = hand;
    }
    
}
