﻿using Parsec.Common;
using Parsec.Shaiya.NpcQuest;

namespace Imgeneus.World.Tests.NpcTests
{
    public class TestNpc : BaseNpc
    {
        public MerchantType MerchantType { get; set; }
        public TestNpc(Format format = Format.EP8) : base(format)
        {
        }
    }
}
