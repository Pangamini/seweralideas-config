#nullable enable
using System;
using NUnit.Framework;
using UnityEngine;

namespace SeweralIdeas.Config.Tests
{
    public class EnumConfigFieldTests
    {
        public enum Mode { Off, Slow, Fast }

        [Flags]
        public enum Layers { None = 0, Ground = 1, Water = 2, Air = 4, Wet = Water | 8 }

        private EnumConfigField m_field = null!;

        [SetUp]
        public void SetUp() => m_field = ScriptableObject.CreateInstance<EnumConfigField>();

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(m_field);

        [Test]
        public void WithoutAType_AnyStringIsAccepted()
        {
            Assert.IsNull(m_field.EnumType);
            Assert.IsTrue(m_field.SetStringValue("whatever"));
            Assert.AreEqual("whatever", m_field.Value);
        }

        [Test]
        public void StoredTypeName_ResolvesNestedEnums()
        {
            m_field.SetEnumType(typeof(Mode));
            Assert.AreEqual(typeof(Mode), m_field.EnumType);
            Assert.AreEqual(typeof(Mode), EnumConfigField.ResolveType(EnumConfigField.FormatTypeName(typeof(Mode))));
        }

        [Test]
        public void TypeThatIsNotFound_AcceptsAnyString()
        {
            Assert.IsNull(EnumConfigField.ResolveType("No.Such.Enum, NoAssembly"));
            Assert.IsNull(EnumConfigField.ResolveType("System.String, mscorlib")); // a type, but not an enum
        }

        [Test]
        public void Option_IsStoredAsItsCanonicalName()
        {
            m_field.SetEnumType(typeof(Mode));
            Assert.IsTrue(m_field.SetStringValue("fAsT"));
            Assert.AreEqual("Fast", m_field.Value);
            Assert.IsTrue(m_field.StringValueEquals("FAST"));
        }

        [Test]
        public void NumbersAndUnknownNames_AreRejected_AndTheValueIsKept()
        {
            m_field.SetEnumType(typeof(Mode));
            m_field.SetStringValue("Slow");

            Assert.IsFalse(m_field.SetStringValue("1"));
            Assert.IsFalse(m_field.SetStringValue("Medium"));
            Assert.IsFalse(m_field.SetStringValue(""));
            Assert.AreEqual("Slow", m_field.Value);
        }

        [Test]
        public void Flags_AreJoinedInTheCanonicalOrder()
        {
            m_field.SetEnumType(typeof(Layers));
            Assert.IsTrue(m_field.SetStringValue("air,  GROUND"));
            Assert.AreEqual("Ground, Air", m_field.Value);
        }

        [Test]
        public void Flags_ZeroIsAccepted_AndUnknownPartsAreNot()
        {
            m_field.SetEnumType(typeof(Layers));
            Assert.IsTrue(m_field.SetStringValue("0"));
            Assert.AreEqual("None", m_field.Value);
            Assert.IsFalse(m_field.SetStringValue("Ground, Lava"));
            Assert.IsFalse(m_field.SetStringValue("Ground,"));
            Assert.IsFalse(m_field.SetStringValue("3"));
        }

        [Test]
        public void Flags_IsActive_MeansContained()
        {
            Layers value = Layers.Ground | Layers.Water;
            Assert.IsTrue(EnumConfigField.IsActive(typeof(Layers), value, Layers.Water));
            Assert.IsFalse(EnumConfigField.IsActive(typeof(Layers), value, Layers.Air));
            Assert.IsFalse(EnumConfigField.IsActive(typeof(Layers), value, Layers.Wet)); // needs the bit 8 too
            Assert.IsFalse(EnumConfigField.IsActive(typeof(Layers), value, Layers.None)); // 0 is on only for the value 0
            Assert.IsTrue(EnumConfigField.IsActive(typeof(Layers), Layers.None, Layers.None));
        }

        [Test]
        public void Flags_SetFlag_TurnsAnOptionOnAndOff()
        {
            object value = Layers.Ground;
            value = EnumConfigField.SetFlag(typeof(Layers), value, Layers.Air, true);
            Assert.AreEqual(Layers.Ground | Layers.Air, value);
            value = EnumConfigField.SetFlag(typeof(Layers), value, Layers.Ground, false);
            Assert.AreEqual(Layers.Air, value);
        }

        [Test]
        public void NonFlags_IsActive_MeansEqual()
        {
            Assert.IsTrue(EnumConfigField.IsActive(typeof(Mode), Mode.Slow, Mode.Slow));
            Assert.IsFalse(EnumConfigField.IsActive(typeof(Mode), Mode.Slow, Mode.Fast));
        }

        [Test]
        public void TypedAccess_ReadsAndWritesTheEnum_AndRefusesAnotherOne()
        {
            m_field.SetEnumType(typeof(Mode));
            Assert.IsTrue(m_field.SetValue(Mode.Fast));
            Assert.AreEqual("Fast", m_field.Value);
            Assert.IsTrue(m_field.TryGetValue(out Mode mode));
            Assert.AreEqual(Mode.Fast, mode);

            Assert.IsFalse(m_field.TryGetValue(out Layers _));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("not Layers"));
            Assert.IsFalse(m_field.SetValue(Layers.Air));
        }

        [Test]
        public void Subscribe_CallsWithTheEnum_UntilDisposed()
        {
            m_field.SetEnumType(typeof(Mode));
            m_field.SetValue(Mode.Off);

            var received = new System.Collections.Generic.List<Mode>();
            using(m_field.Subscribe<Mode>(received.Add))
            {
                m_field.SetValue(Mode.Slow);
                m_field.SetValue(Mode.Slow); // no change
                m_field.SetValue(Mode.Fast);
            }
            m_field.SetValue(Mode.Off);

            CollectionAssert.AreEqual(new[] { Mode.Off, Mode.Slow, Mode.Fast }, received);
        }
    }
}
