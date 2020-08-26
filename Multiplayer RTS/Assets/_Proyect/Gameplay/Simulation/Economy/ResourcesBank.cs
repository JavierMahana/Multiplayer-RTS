using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourcesBank 
{
    public enum StartingResources
    {
        STANDART,
        HIGH,
        MAX
    }
    private const int STD_FOOD = 200;
    private const int STD_WOOD = 200;
    private const int STD_GOLD = 100;
    private const int STD_STONE = 200;

    private const int HIGH_FOOD = 400;
    private const int HIGH_WOOD = 400;
    private const int HIGH_GOLD = 200;
    private const int HIGH_STONE = 300;

    private const int MAX_FOOD = int.MaxValue;
    private const int MAX_WOOD = int.MaxValue;
    private const int MAX_GOLD = int.MaxValue;
    private const int MAX_STONE = int.MaxValue;



    public ResourcesBank()
    {
        foodCount = 0;
        woodCount = 0;
        goldCount = 0;
        stoneCount = 0;
    }
    public ResourcesBank(int food, int wood, int gold, int stone)
    {
        foodCount = food;
        woodCount = wood;
        goldCount = gold;
        stoneCount = stone;
    }
    public ResourcesBank(StartingResources startingResources)
    {
        switch (startingResources)
        {
            case StartingResources.STANDART:
                foodCount = STD_FOOD;
                woodCount = STD_WOOD;
                goldCount = STD_GOLD;
                stoneCount = STD_STONE;
                break;
            case StartingResources.HIGH:
                foodCount = HIGH_FOOD;
                woodCount = HIGH_WOOD;
                goldCount = HIGH_GOLD;
                stoneCount = HIGH_STONE;
                break;
            case StartingResources.MAX:
                foodCount = MAX_FOOD;
                woodCount = MAX_WOOD;
                goldCount = MAX_GOLD;
                stoneCount = MAX_STONE;
                break;
            default:
                foodCount = 0;
                woodCount = 0;
                goldCount = 0;
                stoneCount = 0;

                Debug.LogWarning("you init the resources bank with a no valid starting resources type. The bank is initialized empty");
                break;
        }
    }

    public bool CanAddResources(AddResources add)
    {
        int food = add.food;
        int wood = add.wood;
        int gold = add.gold;
        int stone = add.stone;
        return CanAddResources(food, wood, gold, stone);

    }
    public bool CanAddResources(int food, int wood, int gold, int stone)
    {

        if ((long)foodCount + (long)food > (long)int.MaxValue)
        {
            return false;
        }
        else if ((long)woodCount + (long)wood > (long)int.MaxValue)
        {
            return false;
        }
        else if ((long)goldCount + (long)gold > (long)int.MaxValue)
        {
            return false;
        }
        else if ((long)stoneCount + (long)stone > (long)int.MaxValue)
        {
            return false;
        }
        else
            return true;
    }


    public bool CanRemoveResources(RemoveResources remove)
    {
        int food = remove.food;
        int wood = remove.wood;
        int gold = remove.gold;
        int stone = remove.stone;

        return CanRemoveResources(food, wood, gold, stone);
    }
    public bool CanRemoveResources(int food, int wood, int gold, int stone)
    {
        if (foodCount - food < 0)
        {
            return false;
        }
        else if (woodCount - wood < 0)
        {
            return false;
        }
        else if (goldCount - gold < 0)
        {
            return false;
        }
        else if (stoneCount - stone < 0)
        {
            return false;
        }
        else
            return true;

    }

    public int foodCount;
    public int woodCount;
    public int goldCount;
    public int stoneCount;
}
