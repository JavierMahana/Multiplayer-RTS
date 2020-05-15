using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using FixMath.NET;

public static class FormationSlots
{
    public readonly static Dictionary<int2, FractionalHex[]> Slots = new Dictionary<int2, FractionalHex[]> 
    {

        {new int2(9, 9),

            new FractionalHex[9] {
            new FractionalHex((Fix64)(0.1875)   , (Fix64)(-0.375)),
            new FractionalHex((Fix64)(-0.25)    , (Fix64)(-0.25)),
            new FractionalHex((Fix64)(0.5)      , (Fix64)(-0.25)),
            new FractionalHex((Fix64)(-0.28125) , (Fix64)(0)),
            new FractionalHex((Fix64)(0)        , (Fix64)(0)),
            new FractionalHex((Fix64)(0.28125)  , (Fix64)(0)),
            new FractionalHex((Fix64)(-0.5)     , (Fix64)(0.25)),
            new FractionalHex((Fix64)(0.25)     , (Fix64)(0.25)),
            new FractionalHex((Fix64)(-0.1875)  , (Fix64)(0.375))}
        },
        
        { new int2(8, 9),
            new FractionalHex[8] {

            new FractionalHex((Fix64)(0.1875)    , (Fix64)(-0.375)),
            new FractionalHex((Fix64)(-0.25)     , (Fix64)(-0.25)),
            new FractionalHex((Fix64)(0.5)       , (Fix64)(-0.25)),
            new FractionalHex((Fix64)(-0.2109375), (Fix64)(0)),            
            new FractionalHex((Fix64)(0.2109375) , (Fix64)(0)),
            new FractionalHex((Fix64)(-0.5)      , (Fix64)(0.25)),
            new FractionalHex((Fix64)(0.25)      , (Fix64)(0.25)),
            new FractionalHex((Fix64)(-0.1875)   , (Fix64)(0.375))}
        },



        {new int2(7, 9),

            new FractionalHex[7] {
            new FractionalHex((Fix64)(0)     , (Fix64)(-0.375)),
            new FractionalHex((Fix64)(0.375) , (Fix64)(-0.375)),
            new FractionalHex((Fix64)(-0.375), (Fix64)(0)),
            new FractionalHex((Fix64)(0)     , (Fix64)(0)),
            new FractionalHex((Fix64)(0.375) , (Fix64)(0)),
            new FractionalHex((Fix64)(-0.375), (Fix64)(0.375)),
            new FractionalHex((Fix64)(0)     , (Fix64)(0.375))}
        },

        {new int2(6, 9),

            new FractionalHex[6] {
            new FractionalHex((Fix64)(0)     , (Fix64)(-0.375)),
            new FractionalHex((Fix64)(0.375) , (Fix64)(-0.375)),
            new FractionalHex((Fix64)(-0.375), (Fix64)(0)),
            new FractionalHex((Fix64)(0.375) , (Fix64)(0)),
            new FractionalHex((Fix64)(-0.375), (Fix64)(0.375)),
            new FractionalHex((Fix64)(0)     , (Fix64)(0.375))}
        },

        {new int2(5, 9),

            new FractionalHex[5] {
            new FractionalHex((Fix64)(0)     , (Fix64)(-0.28125)),
            new FractionalHex((Fix64)(0.375) , (Fix64)(-0.1875)),
            new FractionalHex((Fix64)(0)     , (Fix64)(0)),
            new FractionalHex((Fix64)(-0.375), (Fix64)(0.1875)),
            new FractionalHex((Fix64)(0)     , (Fix64)(0.28125))}
        },

        {new int2(4, 9),

            new FractionalHex[4] {
            new FractionalHex((Fix64)(0)       , (Fix64)(-0.28125)),
            new FractionalHex((Fix64)(0.28125) , (Fix64)(-0.140625)),
            new FractionalHex((Fix64)(-0.28125), (Fix64)(0.140625)),
            new FractionalHex((Fix64)(0)       , (Fix64)(0.28125))}
        },

        {new int2(3, 9),

            new FractionalHex[3] {
            new FractionalHex((Fix64)(0.1046875) , (Fix64)(-0.2109375)),
            new FractionalHex((Fix64)(-0.2109375), (Fix64)(0.1046875)),
            new FractionalHex((Fix64)(0.1046875) , (Fix64)(0.1046875))}
        },

        {new int2(2, 9),

            new FractionalHex[2] {
            new FractionalHex((Fix64)(-0.1046875), (Fix64)(-0.1046875)),
            new FractionalHex((Fix64)(0.1046875) , (Fix64)(0.1046875))}
        },

        {new int2(1, 9),

            new FractionalHex[1] {
            new FractionalHex((Fix64)(0), (Fix64)(0))}
        },
    };
}
