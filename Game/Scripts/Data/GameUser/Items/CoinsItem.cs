using System.Collections;
using System.Collections.Generic;
using Template.GameUsers;
using UnityEngine;

namespace GameCore.Data
{
    public class CoinsItem : Item
    {
        public override bool IsSerializedAmountOnly => true;
        public static string KeyStatic => "coins";
        public override string Key => KeyStatic;
    }
}