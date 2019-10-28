using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

//public class Map 
//{
//    /// <summary>
//    /// testmap ctor
//    /// </summary>
//    public Map()
//    {
//        Texture testTexture = //Resources.Load<Texture>("")
//        HexTextures = new Dictionary<Hex, Texture>();
//        HexWalkableFlags = new Dictionary<Hex, bool>();
//        for (int r = 0; r < 10; r++)
//        {
//            int r_offset = (int)Mathf.Floor(r / 2); // or r>>1
//            for (int q = -r_offset; q < 10 - r_offset; q++)
//            {
//                Hex hex = new Hex(q, r, -q - r);
//                //HexTextures.Add(hex, );
//                HexWalkableFlags.Add(hex, true);
//            }
//        }
//    }
//    public Map(Dictionary<Hex, bool> hexWalkableFlags, Dictionary<Hex, Texture> hexTextures)
//    {
//        HexWalkableFlags = hexWalkableFlags;
//        HexTextures = hexTextures;
//    }
//    public readonly Dictionary<Hex, Texture> HexTextures;
//    //luego hacer un bitmask
//    public readonly Dictionary<Hex, bool> HexWalkableFlags;

//    public static Map Active { 
//        get {
//            if (active != null) active = Test;
//            return active;
//        }
//        set => active = value;
//    }
//    private static Map active;

//    public static Map Test { get {
//            if (test == null) 
//            {
//                test = new Map();
//            }
//            return test;
//        } }
//    private static Map test;
    
//    //public Dictionary<Hex, int2>



//}
