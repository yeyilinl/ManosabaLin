using System;
using System.Buffers;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Godot.Bridge;
using Godot.NativeInterop;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Microsoft.CodeAnalysis;
using MinionLib;
using MinionLib.Action;
using MinionLib.Action.GameActions;
using MinionLib.Commands;
using MinionLib.Component.Core;
using MinionLib.Component.Extensions;
using MinionLib.Component.Interfaces;
using MinionLib.Initialization;
using MinionLib.Layout;
using MinionLib.Minion;
using MinionLib.RightClick;
using MinionLib.RightClick.Easy;
using MinionLib.Targeting;
using MinionLib.Targeting.Pets;
using MinionLib.Targeting.Utilities;
using MinionLib.Utilities.BetterExtraArgs;
using MinionLib.Utilities.CustomGlowColor;
using MinionLib.Utilities.DescriptionPostProcess;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints)]
[assembly: TargetFramework(".NETCoreApp,Version=v9.0", FrameworkDisplayName = ".NET 9.0")]
[assembly: AssemblyCompany("FuYnAloft")]
[assembly: AssemblyConfiguration("ExportRelease")]
[assembly: AssemblyDescription("A framework library for Slay the Spire 2 modding. Provides standardized systems for implementing \"Minions\" (summoned allies) and \"Component-based Cards\" (modular card logic). Includes source generators to minimize boilerplate.")]
[assembly: AssemblyFileVersion("0.5.2.0")]
[assembly: AssemblyInformationalVersion("0.5.2+44589da11eaac7aaa5c6fe12b8e97396db04a9c2")]
[assembly: AssemblyProduct("MinionLib")]
[assembly: AssemblyTitle("MinionLib")]
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/FuYnAloft/MinionLib")]
[assembly: AssemblyHasScripts(new Type[] { typeof(MainFile) })]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: AssemblyVersion("0.5.2.0")]
[module: UnverifiableCode]
[module: RefSafetyRules(11)]
[CompilerGenerated]
internal sealed class <>z__ReadOnlySingleElementList<T> : IEnumerable, ICollection, IList, IEnumerable<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, ICollection<T>, IList<T>
{
	private sealed class Enumerator : IDisposable, IEnumerator, IEnumerator<T>
	{
		[CompilerGenerated]
		private readonly T _item;

		[CompilerGenerated]
		private bool _moveNextCalled;

		object IEnumerator.Current => _item;

		T IEnumerator<T>.Current => _item;

		public Enumerator(T item)
		{
			_item = item;
		}

		bool IEnumerator.MoveNext()
		{
			if (!_moveNextCalled)
			{
				return _moveNextCalled = true;
			}
			return false;
		}

		void IEnumerator.Reset()
		{
			_moveNextCalled = false;
		}

		void IDisposable.Dispose()
		{
		}
	}

	[CompilerGenerated]
	private readonly T _item;

	int ICollection.Count => 1;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => this;

	object? IList.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	bool IList.IsFixedSize => true;

	bool IList.IsReadOnly => true;

	int IReadOnlyCollection<T>.Count => 1;

	T IReadOnlyList<T>.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
	}

	int ICollection<T>.Count => 1;

	bool ICollection<T>.IsReadOnly => true;

	T IList<T>.this[int index]
	{
		get
		{
			if (index != 0)
			{
				throw new IndexOutOfRangeException();
			}
			return _item;
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public <>z__ReadOnlySingleElementList(T item)
	{
		_item = item;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(_item);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		array.SetValue(_item, index);
	}

	int IList.Add(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Clear()
	{
		throw new NotSupportedException();
	}

	bool IList.Contains(object? value)
	{
		return EqualityComparer<T>.Default.Equals(_item, (T)value);
	}

	int IList.IndexOf(object? value)
	{
		if (!EqualityComparer<T>.Default.Equals(_item, (T)value))
		{
			return -1;
		}
		return 0;
	}

	void IList.Insert(int index, object? value)
	{
		throw new NotSupportedException();
	}

	void IList.Remove(object? value)
	{
		throw new NotSupportedException();
	}

	void IList.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return new Enumerator(_item);
	}

	void ICollection<T>.Add(T item)
	{
		throw new NotSupportedException();
	}

	void ICollection<T>.Clear()
	{
		throw new NotSupportedException();
	}

	bool ICollection<T>.Contains(T item)
	{
		return EqualityComparer<T>.Default.Equals(_item, item);
	}

	void ICollection<T>.CopyTo(T[] array, int arrayIndex)
	{
		array[arrayIndex] = _item;
	}

	bool ICollection<T>.Remove(T item)
	{
		throw new NotSupportedException();
	}

	int IList<T>.IndexOf(T item)
	{
		if (!EqualityComparer<T>.Default.Equals(_item, item))
		{
			return -1;
		}
		return 0;
	}

	void IList<T>.Insert(int index, T item)
	{
		throw new NotSupportedException();
	}

	void IList<T>.RemoveAt(int index)
	{
		throw new NotSupportedException();
	}
}
namespace Microsoft.CodeAnalysis
{
	[CompilerGenerated]
	[Embedded]
	internal sealed class EmbeddedAttribute : Attribute
	{
	}
}
namespace System.Runtime.CompilerServices
{
	[CompilerGenerated]
	[Embedded]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
	internal sealed class ExtensionMarkerAttribute : Attribute
	{
		private readonly string <Name>k__BackingField;

		public string Name => <Name>k__BackingField;

		public ExtensionMarkerAttribute(string name)
		{
			<Name>k__BackingField = name;
		}
	}
}
namespace GodotPlugins.Game
{
	internal static class Main
	{
		[UnmanagedCallersOnly(EntryPoint = "godotsharp_game_main_init")]
		private static godot_bool InitializeFromGameProject(nint godotDllHandle, nint outManagedCallbacks, nint unmanagedCallbacks, int unmanagedCallbacksSize)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected O, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			try
			{
				DllImportResolver resolver = new GodotDllImportResolver((IntPtr)godotDllHandle).OnResolveDllImport;
				NativeLibrary.SetDllImportResolver(typeof(GodotObject).Assembly, resolver);
				NativeFuncs.Initialize((IntPtr)unmanagedCallbacks, unmanagedCallbacksSize);
				ManagedCallbacks.Create((IntPtr)outManagedCallbacks);
				ScriptManagerBridge.LookupScriptsInAssembly(typeof(Main).Assembly);
				return (godot_bool)1;
			}
			catch (Exception value)
			{
				Console.Error.WriteLine(value);
				return GodotBoolExtensions.ToGodotBool(false);
			}
		}
	}
}
namespace MinionLib
{
	[ModInitializer("Initialize")]
	[ScriptPath("res://MainFile.cs")]
	public class MainFile : Node
	{
		public class MethodName : MethodName
		{
			public static readonly StringName Initialize = StringName.op_Implicit("Initialize");
		}

		public class PropertyName : PropertyName
		{
		}

		public class SignalName : SignalName
		{
		}

		public const string ModId = "MinionLib";

		public static void Initialize()
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			new Harmony("MinionLib").PatchAll();
			MinionHookInitializer.Initialize();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static List<MethodInfo> GetGodotMethodList()
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			return new List<MethodInfo>(1)
			{
				new MethodInfo(MethodName.Initialize, new PropertyInfo((Type)0, StringName.op_Implicit(""), (PropertyHint)0, "", (PropertyUsageFlags)6, false), (MethodFlags)33, (List<PropertyInfo>)null, (List<Variant>)null)
			};
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool InvokeGodotClassMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				Initialize();
				ret = default(godot_variant);
				return true;
			}
			return ((Node)this).InvokeGodotClassMethod(ref method, args, ref ret);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		internal static bool InvokeGodotClassStaticMethod(in godot_string_name method, NativeVariantPtrArgs args, out godot_variant ret)
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			if ((ref method) == MethodName.Initialize && ((NativeVariantPtrArgs)(ref args)).Count == 0)
			{
				Initialize();
				ret = default(godot_variant);
				return true;
			}
			ret = default(godot_variant);
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override bool HasGodotClassMethod(in godot_string_name method)
		{
			if ((ref method) == MethodName.Initialize)
			{
				return true;
			}
			return ((Node)this).HasGodotClassMethod(ref method);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void SaveGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).SaveGodotObjectData(info);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		protected override void RestoreGodotObjectData(GodotSerializationInfo info)
		{
			((GodotObject)this).RestoreGodotObjectData(info);
		}
	}
	internal static class DebugLogger
	{
		[Conditional("DEBUG")]
		internal static void Debug(string message)
		{
			Log.Info("[MinionLib] " + message, 2);
		}

		[Conditional("DEBUG")]
		internal static void Debug(string module, string message)
		{
			Log.Info($"[{"MinionLib"}] [{module}] {message}", 2);
		}
	}
}
namespace MinionLib.Utilities
{
	public class PetsOrderAccessor(Player player) : IDisposable
	{
		private readonly int _count = GetRawPetsList(player)?.Count ?? 0;

		public readonly List<Creature>? Pets = GetRawPetsList(player);

		private bool _manualRearranged;

		public void Dispose()
		{
			if ((Pets?.Count ?? 0) != _count)
			{
				throw new InvalidOperationException("PetsAccessor should not be used for operations other than reordering");
			}
			PetOrderSnapshotManager.TakeSnapshot(player);
			if (!_manualRearranged)
			{
				MinionAnimCmd.Rearrange();
			}
			GC.SuppressFinalize(this);
		}

		public void SetManualRearranged(bool value = true)
		{
			_manualRearranged = value;
		}

		public static List<Creature>? GetRawPetsList(Player player)
		{
			PlayerCombatState playerCombatState = player.PlayerCombatState;
			return (List<Creature>)((playerCombatState != null) ? playerCombatState.Pets : null);
		}
	}
}
namespace MinionLib.Utilities.DescriptionPostProcess
{
	[HarmonyPatch]
	public static class DescriptionPostProcessPatch
	{
		[HarmonyTargetMethod]
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.Inner(typeof(CardModel), "DescriptionPreviewType");
			return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new Type[3]
			{
				typeof(PileType),
				type,
				typeof(Creature)
			}, (Type[])null);
		}

		[HarmonyPostfix]
		private static void Postfix(CardModel __instance, ref string __result, PileType pileType, int previewType, Creature? target = null)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (__instance is IDescriptionPostProcessCard descriptionPostProcessCard)
			{
				__result = descriptionPostProcessCard.PostProcessDescription(__result, pileType, (DescriptionPreviewType)previewType, target);
			}
		}
	}
	public interface IDescriptionPostProcessCard
	{
		string PostProcessDescription(string description, PileType pileType, DescriptionPreviewType previewType, Creature? target = null);
	}
}
namespace MinionLib.Utilities.CustomGlowColor
{
	[HarmonyPatch(typeof(NHandCardHolder))]
	public static class CustomGlowColorPatch
	{
		[HarmonyPatch("UpdateCard")]
		[HarmonyPostfix]
		private static void UpdateCardPostfix(NHandCardHolder __instance)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (TryGetGlowColor(__instance, out var glowColor))
			{
				NCard cardNode = ((NCardHolder)__instance).CardNode;
				NCardHighlight val = ((cardNode != null) ? cardNode.CardHighlight : null);
				if (val != null)
				{
					ApplyGlowColor((CanvasItem)(object)val, glowColor);
				}
			}
		}

		[HarmonyPatch("Flash")]
		[HarmonyPostfix]
		private static void FlashPostfix(NHandCardHolder __instance)
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			if (TryGetGlowColor(__instance, out var glowColor))
			{
				Control nodeOrNull = ((Node)__instance).GetNodeOrNull<Control>(NodePath.op_Implicit("Flash"));
				if (nodeOrNull != null)
				{
					ApplyGlowColor((CanvasItem)(object)nodeOrNull, glowColor);
				}
			}
		}

		private static bool TryGetGlowColor(NHandCardHolder holder, out Color glowColor)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			glowColor = default(Color);
			NCard cardNode = ((NCardHolder)holder).CardNode;
			if (!(((cardNode != null) ? cardNode.Model : null) is ICustomGlowColorCard { GlowColor: var glowColor2 }))
			{
				return false;
			}
			if (!glowColor2.HasValue)
			{
				return false;
			}
			glowColor = glowColor2.Value;
			return true;
		}

		private static void ApplyGlowColor(CanvasItem canvasItem, Color glowColor)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			canvasItem.Modulate = new Color(glowColor.R, glowColor.G, glowColor.B, canvasItem.Modulate.A);
		}
	}
	public interface ICustomGlowColorCard
	{
		Color? GlowColor { get; }
	}
}
namespace MinionLib.Utilities.BetterExtraArgs
{
	[HarmonyPatch]
	public static class BetterExtraArgsPatch
	{
		private static MethodBase TargetMethod()
		{
			Type type = AccessTools.Inner(typeof(CardModel), "DescriptionPreviewType");
			return AccessTools.Method(typeof(CardModel), "GetDescriptionForPile", new Type[3]
			{
				typeof(PileType),
				type,
				typeof(Creature)
			}, (Type[])null);
		}

		private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			List<CodeInstruction> codes = instructions.ToList();
			MethodInfo targetMethod = AccessTools.Method(typeof(CardModel), "AddExtraArgsToDescription", (Type[])null, (Type[])null);
			MethodInfo helperMethod = AccessTools.Method(typeof(BetterExtraArgsPatch), "TryBetterAddExtraArgs", (Type[])null, (Type[])null);
			bool found = false;
			for (int i = 0; i < codes.Count; i++)
			{
				yield return codes[i];
				if (!found && CodeInstructionExtensions.Calls(codes[i], targetMethod))
				{
					found = true;
					CodeInstruction loadDescriptionInst = codes[i - 1].Clone();
					loadDescriptionInst.labels.Clear();
					yield return new CodeInstruction(OpCodes.Ldarg_0, (object)null);
					yield return loadDescriptionInst;
					yield return new CodeInstruction(OpCodes.Ldarg_1, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_2, (object)null);
					yield return new CodeInstruction(OpCodes.Ldarg_3, (object)null);
					yield return new CodeInstruction(OpCodes.Call, (object)helperMethod);
				}
			}
		}

		private static void TryBetterAddExtraArgs(CardModel thisCard, LocString description, PileType pileType, int previewType, Creature? target = null)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (thisCard is IBetterAddExtraArgsCard betterAddExtraArgsCard)
			{
				betterAddExtraArgsCard.BetterAddExtraArgsToDescription(description, pileType, (DescriptionPreviewType)previewType, target);
			}
		}
	}
	public enum DescriptionPreviewType
	{
		None,
		Upgrade
	}
	public interface IBetterAddExtraArgsCard
	{
		void BetterAddExtraArgsToDescription(LocString description, PileType pileType, DescriptionPreviewType previewType, Creature? target = null);
	}
}
namespace MinionLib.Targeting
{
	public abstract class CustomTargetType : ICustomTargetType
	{
		public abstract bool IsSingleTarget { get; }

		public bool IsValidTargetPreview(Creature target)
		{
			return IsValidTarget(target);
		}

		public virtual bool IsValidTarget(CardModel card, Creature target)
		{
			return IsValidTarget(target);
		}

		public virtual bool IsValidTarget(PotionModel potion, Creature target)
		{
			return IsValidTarget(target);
		}

		public virtual bool IsValidTarget(ActionModel action, Creature target)
		{
			return IsValidTarget(target);
		}

		protected abstract bool IsValidTarget(Creature target);
	}
	public static class CustomTargetTypeManager
	{
		private static readonly HashSet<TargetType> RegisteredCustomTypes = new HashSet<TargetType>();

		private static readonly Dictionary<TargetType, ICustomTargetType> CustomTypeDefinitions = new Dictionary<TargetType, ICustomTargetType>(BuiltInTargetType.All);

		public static TargetType Register(ICustomTargetType customTargetType, string @namespace, string name)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			TargetType val = (TargetType)Calculate32BitHash(@namespace + "." + name);
			RegisteredCustomTypes.Add(val);
			CustomTypeDefinitions.Add(val, customTargetType);
			return val;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		public static TargetType Register(ICustomTargetType customTargetType, [CallerArgumentExpression("customTargetType")] string expr = "")
		{
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			string text = new StackTrace().GetFrame(1)?.GetMethod()?.DeclaringType?.FullName?.Split('.').FirstOrDefault() ?? throw new InvalidOperationException("Unable to automatically retrieve the namespace. Please specify it manually.");
			string name = new string(expr.Where((char c) => !char.IsWhiteSpace(c)).ToArray());
			return Register(customTargetType, text, name);
		}

		public static bool IsCustomTargetType(TargetType targetType)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			return RegisteredCustomTypes.Contains(targetType);
		}

		public static bool TryGetCustomTargetType(TargetType targetType, [MaybeNullWhen(false)] out ICustomTargetType customTargetType, bool includeBuiltin = true)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			if (includeBuiltin || IsCustomTargetType(targetType))
			{
				return CustomTypeDefinitions.TryGetValue(targetType, out customTargetType);
			}
			customTargetType = null;
			return false;
		}

		private static int Calculate32BitHash(string str)
		{
			uint num = 2166136261u;
			foreach (char c in str)
			{
				num ^= c;
				num *= 16777619;
			}
			return (int)num;
		}
	}
	public interface ICustomTargetType
	{
		bool IsSingleTarget { get; }

		bool IsValidTargetPreview(Creature target);

		bool IsValidTarget(CardModel card, Creature target);

		bool IsValidTarget(PotionModel potion, Creature target);

		bool IsValidTarget(ActionModel action, Creature target);
	}
	public static class MinionTargetTypes
	{
		public static readonly TargetType AnyMinion;

		public static readonly TargetType AllMinions;

		public static readonly TargetType Itself;

		public static readonly TargetType AnyCreature;

		public static readonly TargetType AllCreatures;

		public static readonly TargetType AnyMinionOrOwner;

		public static readonly TargetType Void;

		private static TargetType Register(ICustomTargetType type, string name)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return CustomTargetTypeManager.Register(type, "MinionLib", name);
		}

		static MinionTargetTypes()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0082: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			AnyMinion = Register(new AnyMinionTargetType(), "AnyMinion");
			AllMinions = Register(new AllMinionsTargetType(), "AllMinions");
			Itself = Register(new ItselfTargetType(), "Itself");
			AnyCreature = Register(new AnyCreatureTargetType(), "AnyCreature");
			AllCreatures = Register(new AllCreaturesTargetType(), "AllCreatures");
			AnyMinionOrOwner = Register(new AnyMinionOrOwnerTargetType(), "AnyMinionOrOwner");
			Void = Register(new VoidTargetType(), "Void");
		}
	}
}
namespace MinionLib.Targeting.Utilities
{
	public static class BuiltInTargetType
	{
		internal static readonly Dictionary<TargetType, ICustomTargetType> All = new Dictionary<TargetType, ICustomTargetType>
		{
			[(TargetType)0] = new LambdaTargetType(isSingleTarget: false, (Creature _) => false),
			[(TargetType)1] = new LambdaTargetType(isSingleTarget: false, (Creature _) => false, (CardModel card, Creature target) => target.IsAlive && target == card.Owner.Creature, (PotionModel potion, Creature target) => target.IsAlive && target == potion.Owner.Creature, (ActionModel action, Creature target) => target != null && target.IsAlive && target.IsPlayer && (target.Player == ((PowerModel)action).Owner.PetOwner || target == ((PowerModel)action).Owner)),
			[(TargetType)2] = new LambdaTargetType(isSingleTarget: true, delegate(Creature target)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I4
				return target != null && target.IsAlive && (int)target.Side == 2;
			}),
			[(TargetType)3] = new LambdaTargetType(isSingleTarget: false, delegate(Creature target)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I4
				return target != null && target.IsAlive && (int)target.Side == 2;
			}),
			[(TargetType)4] = new LambdaTargetType(isSingleTarget: false, delegate(Creature target)
			{
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0012: Invalid comparison between Unknown and I4
				return target != null && target.IsAlive && (int)target.Side == 2;
			}),
			[(TargetType)5] = new LambdaTargetType(isSingleTarget: true, (Creature target) => target != null && target.IsAlive && target.IsPlayer),
			[(TargetType)6] = new LambdaTargetType(isSingleTarget: true, (Creature _) => true, (CardModel card, Creature target) => target.IsAlive && target != card.Owner.Creature, (PotionModel potion, Creature target) => target.IsAlive && target != potion.Owner.Creature, (ActionModel action, Creature target) => target != null && target.IsAlive && target.IsPlayer && target.Player != ((PowerModel)action).Owner.PetOwner && target != ((PowerModel)action).Owner),
			[(TargetType)7] = new LambdaTargetType(isSingleTarget: false, (Creature _) => true, (CardModel card, Creature target) => target.IsAlive && target != card.Owner.Creature, (PotionModel potion, Creature target) => target.IsAlive && target != potion.Owner.Creature, (ActionModel action, Creature target) => target != null && target.IsAlive && target.IsPlayer && target.Player != ((PowerModel)action).Owner.PetOwner && target != ((PowerModel)action).Owner),
			[(TargetType)8] = new LambdaTargetType(isSingleTarget: true, (Creature _) => false),
			[(TargetType)9] = new LambdaTargetType(isSingleTarget: true, delegate(Creature target)
			{
				if (target != null && target.IsAlive && target.IsPet)
				{
					Player petOwner = target.PetOwner;
					return target == ((petOwner != null) ? petOwner.Osty : null);
				}
				return false;
			})
		};

		public static ICustomTargetType From(TargetType targetType)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			if (!All.TryGetValue(targetType, out ICustomTargetType value))
			{
				throw new ArgumentOutOfRangeException("targetType", targetType, $"Unsupported TargetType: {targetType}");
			}
			return value;
		}
	}
	public class DifferenceTargetType(ICustomTargetType original, ICustomTargetType exclude, bool? overrideIsSingleTarget = null) : ICustomTargetType
	{
		public bool IsSingleTarget
		{
			get
			{
				bool? flag = overrideIsSingleTarget;
				if (!flag.HasValue)
				{
					if (!original.IsSingleTarget)
					{
						return exclude.IsSingleTarget;
					}
					return true;
				}
				return flag == true;
			}
		}

		public bool IsValidTargetPreview(Creature target)
		{
			if (original.IsValidTargetPreview(target))
			{
				return !exclude.IsValidTargetPreview(target);
			}
			return false;
		}

		public bool IsValidTarget(CardModel card, Creature target)
		{
			if (original.IsValidTarget(card, target))
			{
				return !exclude.IsValidTarget(card, target);
			}
			return false;
		}

		public bool IsValidTarget(PotionModel potion, Creature target)
		{
			if (original.IsValidTarget(potion, target))
			{
				return !exclude.IsValidTarget(potion, target);
			}
			return false;
		}

		public bool IsValidTarget(ActionModel action, Creature target)
		{
			if (original.IsValidTarget(action, target))
			{
				return !exclude.IsValidTarget(action, target);
			}
			return false;
		}
	}
	public class IntersectionTargetType(params ICustomTargetType[] targetTypes) : ICustomTargetType
	{
		public bool IsSingleTarget => targetTypes.Any((ICustomTargetType targetType) => targetType.IsSingleTarget);

		public bool IsValidTargetPreview(Creature target)
		{
			return targetTypes.All((ICustomTargetType targetType) => targetType.IsValidTargetPreview(target));
		}

		public bool IsValidTarget(CardModel card, Creature target)
		{
			return targetTypes.All((ICustomTargetType targetType) => targetType.IsValidTarget(card, target));
		}

		public bool IsValidTarget(PotionModel potion, Creature target)
		{
			return targetTypes.All((ICustomTargetType targetType) => targetType.IsValidTarget(potion, target));
		}

		public bool IsValidTarget(ActionModel action, Creature target)
		{
			return targetTypes.All((ICustomTargetType targetType) => targetType.IsValidTarget(action, target));
		}
	}
	public class LambdaTargetType(bool isSingleTarget, Func<Creature, bool> generalPredicate, Func<CardModel, Creature, bool>? cardPredicate = null, Func<PotionModel, Creature, bool>? potionPredicate = null, Func<ActionModel, Creature, bool>? actionPredicate = null) : CustomTargetType
	{
		public override bool IsSingleTarget => isSingleTarget;

		protected override bool IsValidTarget(Creature target)
		{
			return generalPredicate(target);
		}

		public override bool IsValidTarget(CardModel card, Creature target)
		{
			return cardPredicate?.Invoke(card, target) ?? generalPredicate(target);
		}

		public override bool IsValidTarget(PotionModel potion, Creature target)
		{
			return potionPredicate?.Invoke(potion, target) ?? generalPredicate(target);
		}

		public override bool IsValidTarget(ActionModel action, Creature target)
		{
			return actionPredicate?.Invoke(action, target) ?? generalPredicate(target);
		}
	}
	public static class SingleTargetTypesUnionManager
	{
		private static readonly Dictionary<ImmutableHashSet<TargetType>, TargetType> Registry;

		private static readonly Dictionary<TargetType, ImmutableHashSet<TargetType>> Components;

		private static (ImmutableHashSet<TargetType>, IEnumerable<ICustomTargetType>) FilterSingleAndSelect(IEnumerable<TargetType> source)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			ImmutableHashSet<TargetType>.Builder builder = ImmutableHashSet.CreateBuilder<TargetType>();
			List<ICustomTargetType> list = new List<ICustomTargetType>();
			foreach (TargetType item in Breakdown(source))
			{
				if (!CustomTargetTypeManager.TryGetCustomTargetType(item, out ICustomTargetType customTargetType))
				{
					Log.Warn($"TargetType '{item}' is not a registered custom target type. Skipping.", 2);
				}
				else if (customTargetType.IsSingleTarget)
				{
					builder.Add(item);
					list.Add(customTargetType);
				}
			}
			return (builder.ToImmutable(), list);
		}

		private static HashSet<TargetType> Breakdown(IEnumerable<TargetType> source)
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			HashSet<TargetType> hashSet = new HashSet<TargetType>();
			foreach (TargetType item in source)
			{
				if (Components.TryGetValue(item, out ImmutableHashSet<TargetType> value))
				{
					foreach (TargetType item2 in value)
					{
						hashSet.Add(item2);
					}
				}
				else
				{
					hashSet.Add(item);
				}
			}
			return hashSet;
		}

		public static TargetType Get(IEnumerable<TargetType> targetTypes)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			TargetType[] source = (targetTypes as TargetType[]) ?? targetTypes.ToArray();
			var (immutableHashSet, source2) = FilterSingleAndSelect(source);
			if (immutableHashSet.IsEmpty)
			{
				return MinionTargetTypes.Void;
			}
			if (immutableHashSet.Count == 1)
			{
				return immutableHashSet.Single();
			}
			if (Registry.TryGetValue(immutableHashSet, out var value))
			{
				return value;
			}
			UnionTargetType customTargetType = new UnionTargetType(source2.ToArray());
			string name = string.Join("|", source.Select(delegate(TargetType t)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return ((Enum)t).ToString("X");
			}));
			TargetType val = CustomTargetTypeManager.Register(customTargetType, "MinionLib-UnionTargetType", name);
			Registry[immutableHashSet] = val;
			Components[val] = immutableHashSet;
			return val;
		}

		public static TargetType GetWithBase(IEnumerable<TargetType> targetTypes, TargetType baseType)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			TargetType val = Get(targetTypes);
			if (val != MinionTargetTypes.Void)
			{
				return val;
			}
			return baseType;
		}

		static SingleTargetTypesUnionManager()
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			Registry = new Dictionary<ImmutableHashSet<TargetType>, TargetType> { [ImmutableHashSet<TargetType>.Empty] = MinionTargetTypes.Void };
			Components = new Dictionary<TargetType, ImmutableHashSet<TargetType>> { [MinionTargetTypes.Void] = ImmutableHashSet<TargetType>.Empty };
		}
	}
	public class UnionTargetType(params ICustomTargetType[] targetTypes) : ICustomTargetType
	{
		public bool IsSingleTarget => targetTypes.Any((ICustomTargetType targetType) => targetType.IsSingleTarget);

		public bool IsValidTargetPreview(Creature target)
		{
			return targetTypes.Any((ICustomTargetType targetType) => targetType.IsValidTargetPreview(target));
		}

		public bool IsValidTarget(CardModel card, Creature target)
		{
			return targetTypes.Any((ICustomTargetType targetType) => targetType.IsValidTarget(card, target));
		}

		public bool IsValidTarget(PotionModel potion, Creature target)
		{
			return targetTypes.Any((ICustomTargetType targetType) => targetType.IsValidTarget(potion, target));
		}

		public bool IsValidTarget(ActionModel action, Creature target)
		{
			return targetTypes.Any((ICustomTargetType targetType) => targetType.IsValidTarget(action, target));
		}
	}
}
namespace MinionLib.Targeting.Pets
{
	public class AllCreaturesTargetType : CustomTargetType
	{
		public override bool IsSingleTarget => false;

		protected override bool IsValidTarget(Creature target)
		{
			return target.IsAlive;
		}
	}
	public class AllMinionsTargetType : ICustomTargetType
	{
		public bool IsSingleTarget => false;

		public bool IsValidTargetPreview(Creature target)
		{
			if (IsValidTarget(target))
			{
				return LocalContext.IsMe(target.PetOwner);
			}
			return false;
		}

		public bool IsValidTarget(CardModel card, Creature target)
		{
			if (IsValidTarget(target))
			{
				return target.PetOwner == card.Owner;
			}
			return false;
		}

		public bool IsValidTarget(PotionModel potion, Creature target)
		{
			if (IsValidTarget(target))
			{
				return target.PetOwner == potion.Owner;
			}
			return false;
		}

		public bool IsValidTarget(ActionModel action, Creature target)
		{
			Creature owner = ((PowerModel)action).Owner;
			if (IsValidTarget(target))
			{
				if (target.PetOwner != owner.PetOwner)
				{
					return target.PetOwner == owner.Player;
				}
				return true;
			}
			return false;
		}

		private static bool IsValidTarget(Creature target)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			if (target != null && target.IsAlive && (int)target.Side == 1 && target.IsPet)
			{
				return target.Monster is MinionModel;
			}
			return false;
		}
	}
	public class AnyCreatureTargetType : CustomTargetType
	{
		public override bool IsSingleTarget => true;

		protected override bool IsValidTarget(Creature target)
		{
			return target.IsAlive;
		}
	}
	public class AnyMinionOrOwnerTargetType : ICustomTargetType
	{
		public bool IsSingleTarget => true;

		public bool IsValidTargetPreview(Creature target)
		{
			if (IsValidTarget(target))
			{
				if (!LocalContext.IsMe(target))
				{
					return LocalContext.IsMe(target.PetOwner);
				}
				return true;
			}
			return false;
		}

		public bool IsValidTarget(CardModel card, Creature target)
		{
			if (IsValidTarget(target))
			{
				if (target.PetOwner != card.Owner)
				{
					return target.Player == card.Owner;
				}
				return true;
			}
			return false;
		}

		public bool IsValidTarget(PotionModel potion, Creature target)
		{
			if (IsValidTarget(target))
			{
				if (target.PetOwner != potion.Owner)
				{
					return target.Player == potion.Owner;
				}
				return true;
			}
			return false;
		}

		public bool IsValidTarget(ActionModel action, Creature target)
		{
			Creature owner = ((PowerModel)action).Owner;
			if (IsValidTarget(target))
			{
				if (target != owner && target.PetOwner != owner.Player && target.Player != owner.PetOwner)
				{
					return target.PetOwner == owner.PetOwner;
				}
				return true;
			}
			return false;
		}

		private static bool IsValidTarget(Creature target)
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Invalid comparison between Unknown and I4
			if (target.IsAlive)
			{
				if (!target.IsPlayer)
				{
					if (target != null && (int)target.Side == 1 && target.IsPet)
					{
						return target.Monster is MinionModel;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}
	public class AnyMinionTargetType : ICustomTargetType
	{
		public bool IsSingleTarget => true;

		public bool IsValidTargetPreview(Creature target)
		{
			if (IsValidTarget(target))
			{
				return LocalContext.IsMe(target.PetOwner);
			}
			return false;
		}

		public bool IsValidTarget(CardModel card, Creature target)
		{
			if (IsValidTarget(target))
			{
				return target.PetOwner == card.Owner;
			}
			return false;
		}

		public bool IsValidTarget(PotionModel potion, Creature target)
		{
			if (IsValidTarget(target))
			{
				return target.PetOwner == potion.Owner;
			}
			return false;
		}

		public bool IsValidTarget(ActionModel action, Creature target)
		{
			Creature owner = ((PowerModel)action).Owner;
			if (IsValidTarget(target))
			{
				if (target.PetOwner != owner.PetOwner)
				{
					return target.PetOwner == owner.Player;
				}
				return true;
			}
			return false;
		}

		private static bool IsValidTarget(Creature target)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I4
			if (target != null && target.IsAlive && (int)target.Side == 1 && target.IsPet)
			{
				return target.Monster is MinionModel;
			}
			return false;
		}
	}
	public class ItselfTargetType : CustomTargetType
	{
		public override bool IsSingleTarget => true;

		protected override bool IsValidTarget(Creature target)
		{
			return false;
		}

		public override bool IsValidTarget(ActionModel action, Creature target)
		{
			return target == ((PowerModel)action).Owner;
		}
	}
	public class VoidTargetType : CustomTargetType
	{
		public override bool IsSingleTarget => true;

		protected override bool IsValidTarget(Creature target)
		{
			return false;
		}
	}
}
namespace MinionLib.Targeting.Patches
{
	[HarmonyPatch]
	public static class CustomTargetTypeCardPatch
	{
		private const string Module = "Targeting";

		private static readonly FieldRef<NTargetManager, TargetType> ValidTargetsTypeRef = AccessTools.FieldRefAccess<NTargetManager, TargetType>("_validTargetsType");

		private static readonly MethodInfo? MouseSingleCreatureTargeting = AccessTools.Method(typeof(NMouseCardPlay), "SingleCreatureTargeting", (Type[])null, (Type[])null);

		private static readonly MethodInfo? ControllerSingleCreatureTargeting = AccessTools.Method(typeof(NControllerCardPlay), "SingleCreatureTargeting", (Type[])null, (Type[])null);

		private static readonly MethodInfo? OnCreatureHoverMethod = AccessTools.Method(typeof(NCardPlay), "OnCreatureHover", (Type[])null, (Type[])null);

		private static readonly MethodInfo? OnCreatureUnhoverMethod = AccessTools.Method(typeof(NCardPlay), "OnCreatureUnhover", (Type[])null, (Type[])null);

		private static readonly MethodInfo? TryPlayCardMethod = AccessTools.Method(typeof(NCardPlay), "TryPlayCard", (Type[])null, (Type[])null);

		private static readonly MethodInfo? CardPlayCleanupMethod = AccessTools.Method(typeof(NCardPlay), "Cleanup", new Type[1] { typeof(bool) }, (Type[])null);

		[HarmonyPatch(typeof(ActionTargetExtensions), "IsSingleTarget")]
		[HarmonyPostfix]
		private static void IsSingleTargetPostfix(TargetType targetType, ref bool __result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			if (CustomTargetTypeManager.TryGetCustomTargetType(targetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				__result = customTargetType.IsSingleTarget;
			}
		}

		[HarmonyPatch(typeof(NTargetManager), "AllowedToTargetCreature")]
		[HarmonyPrefix]
		private static bool AllowedToTargetCreaturePrefix(NTargetManager __instance, Creature creature, ref bool __result)
		{
			if (!CustomTargetTypeManager.TryGetCustomTargetType(ValidTargetsTypeRef.Invoke(__instance), out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			__result = customTargetType.IsValidTargetPreview(creature);
			return false;
		}

		[HarmonyPatch(typeof(CardModel), "IsValidTarget")]
		[HarmonyPrefix]
		private static bool IsValidTargetPrefix(CardModel __instance, Creature? target, ref bool __result)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if (!CustomTargetTypeManager.TryGetCustomTargetType(__instance.TargetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			__result = ((!customTargetType.IsSingleTarget) ? (target == null || customTargetType.IsValidTarget(__instance, target)) : (target != null && customTargetType.IsValidTarget(__instance, target)));
			return false;
		}

		[HarmonyPatch(typeof(NCardPlay), "TryPlayCard")]
		[HarmonyPrefix]
		private static bool TryPlayCardPrefix(NCardPlay __instance, Creature? target)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			CardModel currentCard = GetCurrentCard(__instance);
			if (currentCard == null || !CustomTargetTypeManager.TryGetCustomTargetType(currentCard.TargetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			if (customTargetType.IsSingleTarget && target == null)
			{
				__instance.CancelPlayCard();
				return false;
			}
			Creature val = (customTargetType.IsSingleTarget ? target : null);
			if (!currentCard.CanPlayTargeting(val))
			{
				__instance.CancelPlayCard();
				return false;
			}
			if (!currentCard.TryManualPlay(val))
			{
				__instance.CancelPlayCard();
				return false;
			}
			CardPlayCleanupMethod?.Invoke(__instance, new object[1] { true });
			((GodotObject)__instance).EmitSignal(SignalName.Finished, (Variant[])(object)new Variant[1] { Variant.op_Implicit(true) });
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				NodeUtil.TryGrabFocus((Control)(object)instance.Ui.Hand);
			}
			return false;
		}

		[HarmonyPatch(typeof(NCardPlay), "ShowMultiCreatureTargetingVisuals")]
		[HarmonyPostfix]
		private static void ShowMultiCreatureTargetingVisualsPostfix(NCardPlay __instance)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
			CardModel card = GetCurrentCard(__instance);
			CardModel obj = card;
			if (((obj != null) ? obj.CombatState : null) == null || !CustomTargetTypeManager.TryGetCustomTargetType(card.TargetType, out ICustomTargetType customType, includeBuiltin: false) || customType.IsSingleTarget)
			{
				return;
			}
			NCard cardNode = ((NCardHolder)__instance.Holder).CardNode;
			if (cardNode == null)
			{
				return;
			}
			List<Creature> list = card.CombatState.Creatures.Where((Creature c) => c.IsAlive && customType.IsValidTarget(card, c)).ToList();
			if (list.Count == 1)
			{
				cardNode.SetPreviewTarget(list[0]);
			}
			CardModel model = cardNode.Model;
			PileType? obj2;
			if (model == null)
			{
				obj2 = null;
			}
			else
			{
				CardPile pile = model.Pile;
				obj2 = ((pile != null) ? new PileType?(pile.Type) : ((PileType?)null));
			}
			PileType? val = obj2;
			cardNode.UpdateVisuals(val.GetValueOrDefault(), (CardPreviewMode)3);
			foreach (Creature item in list)
			{
				NCombatRoom instance = NCombatRoom.Instance;
				if (instance != null)
				{
					NCreature creatureNode = instance.GetCreatureNode(item);
					if (creatureNode != null)
					{
						creatureNode.ShowMultiselectReticle();
					}
				}
			}
		}

		[HarmonyPatch(typeof(NMouseCardPlay), "MultiCreatureTargeting")]
		[HarmonyPrefix]
		private static bool MouseMultiCreatureTargetingPrefix(NMouseCardPlay __instance, TargetMode targetMode, ref Task __result)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			CardModel currentCard = GetCurrentCard((NCardPlay)(object)__instance);
			if (currentCard == null || !CustomTargetTypeManager.TryGetCustomTargetType(currentCard.TargetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			if (!customTargetType.IsSingleTarget)
			{
				return true;
			}
			if (MouseSingleCreatureTargeting == null)
			{
				__result = Task.CompletedTask;
				((NCardPlay)__instance).CancelPlayCard();
				return false;
			}
			__result = (Task)MouseSingleCreatureTargeting.Invoke(__instance, new object[2] { targetMode, currentCard.TargetType });
			return false;
		}

		[HarmonyPatch(typeof(NControllerCardPlay), "MultiCreatureTargeting")]
		[HarmonyPrefix]
		private static bool ControllerMultiCreatureTargetingPrefix(NControllerCardPlay __instance)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			CardModel currentCard = GetCurrentCard((NCardPlay)(object)__instance);
			if (currentCard == null || !CustomTargetTypeManager.TryGetCustomTargetType(currentCard.TargetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			if (!customTargetType.IsSingleTarget)
			{
				return true;
			}
			if (ControllerSingleCreatureTargeting == null)
			{
				((NCardPlay)__instance).CancelPlayCard();
				return false;
			}
			TaskHelper.RunSafely((Task)ControllerSingleCreatureTargeting.Invoke(__instance, new object[1] { currentCard.TargetType }));
			return false;
		}

		[HarmonyPatch(typeof(NControllerCardPlay), "SingleCreatureTargeting")]
		[HarmonyPrefix]
		private static bool ControllerSingleCreatureTargetingPrefix(NControllerCardPlay __instance, TargetType targetType, ref Task __result)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (!CustomTargetTypeManager.TryGetCustomTargetType(targetType, out ICustomTargetType _, includeBuiltin: false))
			{
				return true;
			}
			__result = ControllerSingleCustomTargeting(__instance, targetType);
			return false;
		}

		private static async Task ControllerSingleCustomTargeting(NControllerCardPlay cardPlay, TargetType targetType)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			CardModel card = GetCurrentCard((NCardPlay)(object)cardPlay);
			NCard cardNode = ((NCardHolder)((NCardPlay)cardPlay).Holder).CardNode;
			NCombatRoom room = NCombatRoom.Instance;
			if (card == null || cardNode == null || room == null || card.CombatState == null || !CustomTargetTypeManager.TryGetCustomTargetType(targetType, out ICustomTargetType customType, includeBuiltin: false))
			{
				((NCardPlay)cardPlay).CancelPlayCard();
				return;
			}
			NTargetManager targetManager = NTargetManager.Instance;
			Callable onHover = Callable.From<NCreature>((Action<NCreature>)delegate(NCreature creature)
			{
				OnCreatureHoverMethod?.Invoke(cardPlay, new object[1] { creature });
			});
			Callable onUnhover = Callable.From<NCreature>((Action<NCreature>)delegate(NCreature creature)
			{
				OnCreatureUnhoverMethod?.Invoke(cardPlay, new object[1] { creature });
			});
			((GodotObject)targetManager).Connect(SignalName.CreatureHovered, onHover, 0u);
			((GodotObject)targetManager).Connect(SignalName.CreatureUnhovered, onUnhover, 0u);
			targetManager.StartTargeting(targetType, (Control)(object)cardNode, (TargetMode)3, (Func<bool>)delegate
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)cardPlay))
				{
					NControllerManager instance = NControllerManager.Instance;
					return instance == null || !instance.IsUsingController;
				}
				return true;
			}, (Func<Node, bool>)null);
			List<Creature> list = card.CombatState.Creatures.Where((Creature c) => c.IsAlive && customType.IsValidTarget(card, c)).ToList();
			if (list.Count == 0)
			{
				((GodotObject)targetManager).Disconnect(SignalName.CreatureHovered, onHover);
				((GodotObject)targetManager).Disconnect(SignalName.CreatureUnhovered, onUnhover);
				((NCardPlay)cardPlay).CancelPlayCard();
				return;
			}
			IEnumerable<Control> enumerable = (from control in list.Select(delegate(Creature c)
				{
					NCreature creatureNode2 = room.GetCreatureNode(c);
					return (creatureNode2 == null) ? null : creatureNode2.Hitbox;
				})
				where control != null
				select control).Cast<Control>();
			room.RestrictControllerNavigation(enumerable);
			NCreature creatureNode = room.GetCreatureNode(list.First());
			if (creatureNode != null)
			{
				NodeUtil.TryGrabFocus(creatureNode.Hitbox);
			}
			Node val = await targetManager.SelectionFinished();
			if (GodotObject.IsInstanceValid((GodotObject)(object)cardPlay))
			{
				((GodotObject)targetManager).Disconnect(SignalName.CreatureHovered, onHover);
				((GodotObject)targetManager).Disconnect(SignalName.CreatureUnhovered, onUnhover);
				NCreature val2 = (NCreature)(object)((val is NCreature) ? val : null);
				if (val2 != null && TryPlayCardMethod != null)
				{
					TryPlayCardMethod.Invoke(cardPlay, new object[1] { val2.Entity });
				}
				else
				{
					((NCardPlay)cardPlay).CancelPlayCard();
				}
			}
		}

		private static CardModel? GetCurrentCard(NCardPlay cardPlay)
		{
			object? obj = AccessTools.Property(typeof(NCardPlay), "Card")?.GetValue(cardPlay);
			return (CardModel?)((obj is CardModel) ? obj : null);
		}
	}
	[HarmonyPatch]
	public static class CustomTargetTypePotionPatch
	{
		private const string Module = "Targeting";

		private static readonly MethodInfo? TargetNodeMethod = AccessTools.Method(typeof(NPotionHolder), "TargetNode", (Type[])null, (Type[])null);

		private static readonly MethodInfo? ShouldCancelTargetingMethod = AccessTools.Method(typeof(NPotionHolder), "ShouldCancelTargeting", (Type[])null, (Type[])null);

		private static readonly FieldRef<NPotionPopup, NPotionPopupButton> UseButtonRef = AccessTools.FieldRefAccess<NPotionPopup, NPotionPopupButton>("_useButton");

		[HarmonyPatch(typeof(NPotionHolder), "UsePotion")]
		[HarmonyPrefix]
		private static bool UsePotionPrefix(NPotionHolder __instance, ref Task __result)
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			NPotion potion = __instance.Potion;
			PotionModel val = ((potion != null) ? potion.Model : null);
			if (val == null || !CustomTargetTypeManager.TryGetCustomTargetType(val.TargetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			if (!customTargetType.IsSingleTarget)
			{
				val.EnqueueManualUse(val.Owner.Creature);
				NodeUtil.TryGrabFocus((Control)(object)__instance);
				__result = Task.CompletedTask;
				return false;
			}
			__result = UseSingleTargetPotion(__instance, val);
			return false;
		}

		[HarmonyPatch(typeof(NPotionHolder), "TargetNode")]
		[HarmonyPrefix]
		private static bool TargetNodePrefix(NPotionHolder __instance, TargetType targetType, ref Task __result)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			NPotion potion = __instance.Potion;
			PotionModel val = ((potion != null) ? potion.Model : null);
			if (val == null || !CustomTargetTypeManager.TryGetCustomTargetType(targetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				return true;
			}
			__result = TargetNodeCustom(__instance, val, targetType, customTargetType);
			return false;
		}

		[HarmonyPatch(typeof(NPotionPopup), "_Ready")]
		[HarmonyPostfix]
		private static void PopupReadyPostfix(NPotionPopup __instance)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			object? obj = AccessTools.Property(typeof(NPotionPopup), "Potion")?.GetValue(__instance);
			PotionModel val = (PotionModel)((obj is PotionModel) ? obj : null);
			if (val != null && CustomTargetTypeManager.TryGetCustomTargetType(val.TargetType, out ICustomTargetType customTargetType, includeBuiltin: false))
			{
				UseButtonRef.Invoke(__instance).SetLocKey((customTargetType.IsSingleTarget || val.CanThrowAtAlly()) ? "POTION_POPUP.throw" : "POTION_POPUP.drink");
			}
		}

		private static async Task UseSingleTargetPotion(NPotionHolder holder, PotionModel potion)
		{
			if (TargetNodeMethod == null)
			{
				NodeUtil.TryGrabFocus((Control)(object)holder);
				return;
			}
			RunManager.Instance.HoveredModelTracker.OnLocalPotionSelected(potion);
			try
			{
				await (Task)TargetNodeMethod.Invoke(holder, new object[1] { potion.TargetType });
			}
			finally
			{
				RunManager.Instance.HoveredModelTracker.OnLocalPotionDeselected();
			}
		}

		private static async Task TargetNodeCustom(NPotionHolder holder, PotionModel potion, TargetType targetType, ICustomTargetType customType)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			NTargetManager instance = NTargetManager.Instance;
			NControllerManager instance2 = NControllerManager.Instance;
			bool flag = instance2 != null && instance2.IsUsingController;
			Vector2 val = ((Control)holder).GlobalPosition + Vector2.Right * ((Control)holder).Size.X * 0.5f + Vector2.Down * 50f;
			Func<bool> func = null;
			if (ShouldCancelTargetingMethod != null)
			{
				func = () => (bool)ShouldCancelTargetingMethod.Invoke(holder, null);
			}
			instance.StartTargeting(targetType, val, (TargetMode)(flag ? 3 : 2), func, (Func<Node, bool>)((Node node) => IsAllowedPotionTargetNode(node, potion, customType)));
			if (flag && CombatManager.Instance.IsInProgress)
			{
				ICombatState combatState = potion.Owner.Creature.CombatState;
				if (combatState != null)
				{
					List<Control> list = (from hitbox in combatState.Creatures.Where((Creature c) => c.IsAlive && customType.IsValidTarget(potion, c)).Select((Func<Creature, Control>)delegate(Creature c)
						{
							NCombatRoom instance7 = NCombatRoom.Instance;
							if (instance7 == null)
							{
								return (Control)null;
							}
							NCreature creatureNode = instance7.GetCreatureNode(c);
							return (creatureNode == null) ? null : creatureNode.Hitbox;
						})
						where hitbox != null
						select hitbox).Cast<Control>().ToList();
					if (list.Count > 0)
					{
						NCombatRoom instance3 = NCombatRoom.Instance;
						if (instance3 != null)
						{
							instance3.RestrictControllerNavigation((IEnumerable<Control>)list);
						}
						NodeUtil.TryGrabFocus(list[0]);
					}
				}
			}
			else if (flag)
			{
				NRun instance4 = NRun.Instance;
				NMultiplayerPlayerStateContainer val2 = ((instance4 != null) ? instance4.GlobalUi.MultiplayerPlayerContainer : null);
				if (val2 != null)
				{
					List<Control> validPlayerStateHitboxes = GetValidPlayerStateHitboxes(val2, potion, customType);
					if (validPlayerStateHitboxes.Count > 0)
					{
						NodeUtil.TryGrabFocus(validPlayerStateHitboxes[0]);
						val2.LockNavigation();
					}
				}
			}
			try
			{
				Node val3 = await instance.SelectionFinished();
				if (val3 != null)
				{
					Creature val4 = ResolveTargetFromNode(val3);
					if (val4 != null && customType.IsValidTarget(potion, val4))
					{
						potion.EnqueueManualUse(val4);
					}
				}
			}
			finally
			{
				NCombatRoom instance5 = NCombatRoom.Instance;
				if (instance5 != null)
				{
					instance5.EnableControllerNavigation();
				}
				NRun instance6 = NRun.Instance;
				if (instance6 != null)
				{
					instance6.GlobalUi.MultiplayerPlayerContainer.UnlockNavigation();
				}
				NodeUtil.TryGrabFocus((Control)(object)holder);
			}
		}

		private static bool IsAllowedPotionTargetNode(Node node, PotionModel potion, ICustomTargetType customType)
		{
			NCreature val = (NCreature)(object)((node is NCreature) ? node : null);
			if (val != null)
			{
				return customType.IsValidTarget(potion, val.Entity);
			}
			NMultiplayerPlayerState val2 = (NMultiplayerPlayerState)(object)((node is NMultiplayerPlayerState) ? node : null);
			if (val2 != null)
			{
				return customType.IsValidTarget(potion, val2.Player.Creature);
			}
			return false;
		}

		private static List<Control> GetValidPlayerStateHitboxes(NMultiplayerPlayerStateContainer container, PotionModel potion, ICustomTargetType customType)
		{
			List<Control> list = new List<Control>();
			for (int i = 0; i < ((Node)container).GetChildCount(false); i++)
			{
				Node child = ((Node)container).GetChild(i, false);
				NMultiplayerPlayerState val = (NMultiplayerPlayerState)(object)((child is NMultiplayerPlayerState) ? child : null);
				if (val != null && customType.IsValidTarget(potion, val.Player.Creature))
				{
					list.Add((Control)(object)val.Hitbox);
				}
			}
			return list;
		}

		private static Creature? ResolveTargetFromNode(Node node)
		{
			NCreature val = (NCreature)(object)((node is NCreature) ? node : null);
			if (val != null)
			{
				return val.Entity;
			}
			NMultiplayerPlayerState val2 = (NMultiplayerPlayerState)(object)((node is NMultiplayerPlayerState) ? node : null);
			if (val2 != null)
			{
				return val2.Player.Creature;
			}
			return null;
		}
	}
}
namespace MinionLib.RightClick
{
	public interface IRightClickHandler
	{
		int Priority => 0;

		bool Handle(RightClickContext context);
	}
	public record RightClickContext(Player Player, AbstractModel Model, RightClickContext.Payload Extra = default(RightClickContext.Payload))
	{
		public struct Payload : IPacketSerializable
		{
			public bool IsController { get; private set; }

			public string? Meta { get; private set; }

			public Payload(bool isController = false, string? meta = null)
			{
				IsController = isController;
				Meta = meta;
			}

			public void Serialize(PacketWriter writer)
			{
				writer.WriteBool(IsController);
				writer.WriteBool(Meta != null);
				if (Meta != null)
				{
					writer.WriteString(Meta);
				}
			}

			public void Deserialize(PacketReader reader)
			{
				IsController = reader.ReadBool();
				Meta = (reader.ReadBool() ? reader.ReadString() : null);
			}
		}
	}
	public static class RightClickDispatcher
	{
		private sealed class LogIdRightClickHandler : IRightClickHandler
		{
			public bool Handle(RightClickContext context)
			{
				return false;
			}
		}

		private const string Module = "CardRightClick";

		private static readonly List<IRightClickHandler> Handlers;

		public static void Register(IRightClickHandler handler)
		{
			if (!Handlers.Contains(handler))
			{
				Handlers.Add(handler);
				Handlers.Sort((IRightClickHandler a, IRightClickHandler b) => b.Priority.CompareTo(a.Priority));
			}
		}

		public static bool TryDispatch(RightClickContext context)
		{
			foreach (IRightClickHandler handler in Handlers)
			{
				if (handler.Handle(context))
				{
					return true;
				}
			}
			return false;
		}

		static RightClickDispatcher()
		{
			int num = 1;
			List<IRightClickHandler> list = new List<IRightClickHandler>(num);
			CollectionsMarshal.SetCount(list, num);
			CollectionsMarshal.AsSpan(list)[0] = new EasyRightClickableModelHandler();
			Handlers = list;
		}
	}
}
namespace MinionLib.RightClick.Patches
{
	[HarmonyPatch(typeof(NPlayerHand), "AddCardHolder")]
	public static class CardRightClickPatch
	{
		private const string Module = "CardRightClickPatch";

		[HarmonyPostfix]
		private static void Postfix(NHandCardHolder holder)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)holder).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnHolderGuiInput((NCardHolder)(object)holder, inputEvent);
			}), 0u);
			((GodotObject)((NCardHolder)holder).Hitbox).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnHitboxGuiInput((NCardHolder)(object)holder, inputEvent);
			}), 0u);
		}

		private static void OnHolderGuiInput(NCardHolder holder, InputEvent inputEvent)
		{
			InputEventAction val = (InputEventAction)(object)((inputEvent is InputEventAction) ? inputEvent : null);
			if (val != null)
			{
				StringName action = val.Action;
				if (action == MegaInput.cancel && ((InputEvent)val).IsPressed() && ((Control)holder).HasFocus())
				{
					TryHandleRightClick(holder, isController: true);
				}
			}
		}

		private static void OnHitboxGuiInput(NCardHolder holder, InputEvent inputEvent)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			if (val != null && (long)val.ButtonIndex == 2 && ((InputEvent)val).IsPressed())
			{
				TryHandleRightClick(holder, isController: false);
			}
		}

		private static void TryHandleRightClick(NCardHolder holder, bool isController)
		{
			if (((Node)holder).GetViewport().IsInputHandled())
			{
				return;
			}
			NPlayerHand instance = NPlayerHand.Instance;
			if (instance == null)
			{
				return;
			}
			CardModel cardModel = holder.CardModel;
			if (cardModel != null && !instance.InCardPlay && !NTargetManager.Instance.IsInSelection)
			{
				Player me = LocalContext.GetMe(cardModel.CombatState);
				if (me != null && RightClickDispatcher.TryDispatch(new RightClickContext(me, (AbstractModel)(object)cardModel, new RightClickContext.Payload(isController))))
				{
					((Node)holder).GetViewport().SetInputAsHandled();
				}
			}
		}
	}
	[HarmonyPatch(typeof(NPotion), "_Ready")]
	public static class PotionRightClickPatch
	{
		private const string Module = "PotionRightClickPatch";

		[HarmonyPostfix]
		private static void Postfix(NPotion __instance)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)__instance).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnPotionGuiInput(__instance, inputEvent);
			}), 0u);
		}

		private static void OnPotionGuiInput(NPotion potionNode, InputEvent inputEvent)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I8
			if (((Node)potionNode).GetViewport().IsInputHandled())
			{
				return;
			}
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			bool num = val != null && (long)val.ButtonIndex == 2 && ((InputEvent)val).IsReleased();
			InputEventAction val2 = (InputEventAction)(object)((inputEvent is InputEventAction) ? inputEvent : null);
			int num2;
			if (val2 != null)
			{
				StringName action = val2.Action;
				if (action == MegaInput.cancel && ((InputEvent)val2).IsPressed())
				{
					num2 = (((Control)potionNode).HasFocus() ? 1 : 0);
					goto IL_005a;
				}
			}
			num2 = 0;
			goto IL_005a;
			IL_005a:
			bool flag = (byte)num2 != 0;
			if ((num || flag) && !NTargetManager.Instance.IsInSelection)
			{
				PotionModel model = potionNode.Model;
				Player me = LocalContext.GetMe((IPlayerCollection)(object)model.Owner.RunState);
				if (me != null && RightClickDispatcher.TryDispatch(new RightClickContext(me, (AbstractModel)(object)model, new RightClickContext.Payload(flag))))
				{
					((Node)potionNode).GetViewport().SetInputAsHandled();
				}
			}
		}
	}
	[HarmonyPatch(typeof(NPower), "_Ready")]
	public static class PowerRightClickPatch
	{
		private const string Module = "PowerRightClickPatch";

		[HarmonyPostfix]
		private static void Postfix(NPower __instance)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)__instance).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnPowerGuiInput(__instance, inputEvent);
			}), 0u);
		}

		private static void OnPowerGuiInput(NPower powerNode, InputEvent inputEvent)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I8
			if (((Node)powerNode).GetViewport().IsInputHandled())
			{
				return;
			}
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			bool num = val != null && (long)val.ButtonIndex == 2 && ((InputEvent)val).IsReleased();
			InputEventAction val2 = (InputEventAction)(object)((inputEvent is InputEventAction) ? inputEvent : null);
			int num2;
			if (val2 != null)
			{
				StringName action = val2.Action;
				if (action == MegaInput.cancel && ((InputEvent)val2).IsPressed())
				{
					num2 = (((Control)powerNode).HasFocus() ? 1 : 0);
					goto IL_005a;
				}
			}
			num2 = 0;
			goto IL_005a;
			IL_005a:
			bool flag = (byte)num2 != 0;
			if ((num || flag) && !NTargetManager.Instance.IsInSelection)
			{
				PowerModel model = powerNode.Model;
				Player me = LocalContext.GetMe(model.Owner.CombatState);
				if (me != null && RightClickDispatcher.TryDispatch(new RightClickContext(me, (AbstractModel)(object)model, new RightClickContext.Payload(flag))))
				{
					((Node)powerNode).GetViewport().SetInputAsHandled();
				}
			}
		}
	}
	[HarmonyPatch(typeof(NRelic), "_Ready")]
	public static class RelicRightClickPatch
	{
		private const string Module = "RelicRightClickPatch";

		[HarmonyPostfix]
		private static void Postfix(NRelic __instance)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)__instance).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnRelicGuiInput(__instance, inputEvent);
			}), 0u);
		}

		private static void OnRelicGuiInput(NRelic relicNode, InputEvent inputEvent)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I8
			if (((Node)relicNode).GetViewport().IsInputHandled())
			{
				return;
			}
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			bool num = val != null && (long)val.ButtonIndex == 2 && ((InputEvent)val).IsReleased();
			InputEventAction val2 = (InputEventAction)(object)((inputEvent is InputEventAction) ? inputEvent : null);
			int num2;
			if (val2 != null)
			{
				StringName action = val2.Action;
				if (action == MegaInput.cancel && ((InputEvent)val2).IsPressed())
				{
					num2 = (((Control)relicNode).HasFocus() ? 1 : 0);
					goto IL_005a;
				}
			}
			num2 = 0;
			goto IL_005a;
			IL_005a:
			bool flag = (byte)num2 != 0;
			if ((num || flag) && !NTargetManager.Instance.IsInSelection)
			{
				RelicModel model = relicNode.Model;
				Player me = LocalContext.GetMe((IPlayerCollection)(object)model.Owner.RunState);
				if (me != null && RightClickDispatcher.TryDispatch(new RightClickContext(me, (AbstractModel)(object)model, new RightClickContext.Payload(flag))))
				{
					((Node)relicNode).GetViewport().SetInputAsHandled();
				}
			}
		}
	}
}
namespace MinionLib.RightClick.Easy
{
	public class EasyRightClickableModelHandler : IRightClickHandler
	{
		public bool Handle(RightClickContext context)
		{
			if (!(context.Model is IEasyRightClickableModel easyRightClickableModel))
			{
				return false;
			}
			if (!IsValidType(context.Model, context.Player))
			{
				return false;
			}
			if (!easyRightClickableModel.CanHandleRightClickLocal(context))
			{
				return false;
			}
			EasyRightClickCardAction easyRightClickCardAction = new EasyRightClickCardAction(context, CombatManager.Instance.IsInProgress);
			RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue((GameAction)(object)easyRightClickCardAction);
			return true;
		}

		private static bool IsValidType(AbstractModel model, Player player)
		{
			CardModel val = (CardModel)(object)((model is CardModel) ? model : null);
			if (val == null)
			{
				RelicModel val2 = (RelicModel)(object)((model is RelicModel) ? model : null);
				if (val2 == null)
				{
					PowerModel val3 = (PowerModel)(object)((model is PowerModel) ? model : null);
					if (val3 == null)
					{
						PotionModel val4 = (PotionModel)(object)((model is PotionModel) ? model : null);
						if (val4 != null)
						{
							return val4.Owner == player;
						}
						return false;
					}
					return val3.Owner.Player == player || val3.Owner.PetOwner == player || val3.Owner.IsEnemy;
				}
				return val2.Owner == player;
			}
			return val.Owner == player;
		}
	}
	public enum EasyRightClickableModelType
	{
		Card,
		Relic,
		Power,
		Potion
	}
	public class EasyRightClickCardAction : GameAction
	{
		[CompilerGenerated]
		private readonly NetCombatCard <NetCombatCard>k__BackingField;

		public Player Player { get; }

		public RightClickContext.Payload Extra { get; }

		public bool WasEnqueuedInCombat { get; }

		public override ulong OwnerId => Player.NetId;

		public override GameActionType ActionType
		{
			get
			{
				if (WasEnqueuedInCombat)
				{
					return (GameActionType)2;
				}
				return (GameActionType)3;
			}
		}

		public EasyRightClickableModelType Type { get; init; }

		public ModelId ModelId { get; }

		public NetCombatCard NetCombatCard
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return <NetCombatCard>k__BackingField;
			}
			[CompilerGenerated]
			init
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				<NetCombatCard>k__BackingField = value;
			}
		}

		public uint CreatureCombatId { get; init; }

		public uint PotionIndex { get; init; }

		public EasyRightClickCardAction(RightClickContext context, bool isCombatInProgress)
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			((GameAction)this)..ctor();
			WasEnqueuedInCombat = isCombatInProgress;
			Player = context.Player;
			Extra = context.Extra;
			AbstractModel model = context.Model;
			ModelId = model.Id;
			CardModel val = (CardModel)(object)((model is CardModel) ? model : null);
			if (val == null)
			{
				if (!(model is RelicModel))
				{
					PowerModel val2 = (PowerModel)(object)((model is PowerModel) ? model : null);
					if (val2 == null)
					{
						PotionModel val3 = (PotionModel)(object)((model is PotionModel) ? model : null);
						if (val3 == null)
						{
							throw new ArgumentOutOfRangeException("context", model, null);
						}
						Type = EasyRightClickableModelType.Potion;
						int potionSlotIndex = val3.Owner.GetPotionSlotIndex(val3);
						if (potionSlotIndex < 0)
						{
							throw new InvalidOperationException($"Potion {val3} has owner {Player}, but the owner's potion list does not contain it!");
						}
						PotionIndex = (uint)potionSlotIndex;
					}
					else
					{
						Type = EasyRightClickableModelType.Power;
						uint? combatId = val2.Owner.CombatId;
						if (combatId.HasValue)
						{
							CreatureCombatId = combatId.Value;
						}
					}
				}
				else
				{
					Type = EasyRightClickableModelType.Relic;
				}
			}
			else
			{
				Type = EasyRightClickableModelType.Card;
				NetCombatCard = NetCombatCard.FromModel(val);
			}
		}

		public EasyRightClickCardAction(Player player, ModelId modelId, RightClickContext.Payload extra, bool isCombatInProgress)
		{
			Player = player;
			ModelId = modelId;
			Extra = extra;
			WasEnqueuedInCombat = isCombatInProgress;
		}

		protected override async Task ExecuteAction()
		{
			ICombatState combatState = Player.Creature.CombatState;
			if (WasEnqueuedInCombat && combatState == null)
			{
				return;
			}
			AbstractModel model;
			switch (Type)
			{
			case EasyRightClickableModelType.Card:
			{
				NetCombatCard netCombatCard = NetCombatCard;
				CardModel val = ((NetCombatCard)(ref netCombatCard)).ToCardModel();
				CardPile pile = val.Pile;
				if (pile == null || (int)pile.Type != 2)
				{
					return;
				}
				model = (AbstractModel)(object)val;
				break;
			}
			case EasyRightClickableModelType.Relic:
				model = (AbstractModel)(object)Player.Relics.FirstOrDefault((RelicModel r) => ((AbstractModel)r).Id == ModelId);
				break;
			case EasyRightClickableModelType.Power:
			{
				Creature creature = combatState.GetCreature((uint?)CreatureCombatId);
				model = (AbstractModel)(object)((creature != null) ? creature.Powers.FirstOrDefault((PowerModel p) => ((AbstractModel)p).Id == ModelId) : null);
				break;
			}
			case EasyRightClickableModelType.Potion:
				model = (AbstractModel)(object)Player.GetPotionAtSlotIndex((int)PotionIndex);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			if (model != null && !(model.Id != ModelId) && model is IEasyRightClickableModel easyRightClickableModel)
			{
				GameActionPlayerChoiceContext choiceContext = new GameActionPlayerChoiceContext((GameAction)(object)this);
				RightClickContext clickContext = new RightClickContext(Player, model, Extra);
				await easyRightClickableModel.OnRightClick((PlayerChoiceContext)(object)choiceContext, clickContext);
				model.InvokeExecutionFinished();
			}
		}

		public override INetAction ToNetAction()
		{
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			return (INetAction)(object)new NetEasyRightClickCardAction
			{
				Type = Type,
				ModelId = ModelId,
				NetCombatCard = NetCombatCard,
				CreatureCombatId = CreatureCombatId,
				PotionIndex = PotionIndex,
				Extra = Extra,
				WasEnqueuedInCombat = WasEnqueuedInCombat
			};
		}
	}
	public interface IEasyRightClickableModel
	{
		bool CanHandleRightClickLocal(RightClickContext context)
		{
			return true;
		}

		Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext);
	}
	public interface IEasyRightClickableCard : IEasyRightClickableModel
	{
	}
	public interface IEasyRightClickableRelic : IEasyRightClickableModel
	{
	}
	public interface IEasyRightClickablePower : IEasyRightClickableModel
	{
	}
	public interface IEasyRightClickableMonster : IEasyRightClickableModel
	{
	}
	public struct NetEasyRightClickCardAction : INetAction, IPacketSerializable
	{
		public EasyRightClickableModelType Type;

		public ModelId ModelId;

		public NetCombatCard NetCombatCard;

		public uint CreatureCombatId;

		public uint PotionIndex;

		public RightClickContext.Payload Extra;

		public bool WasEnqueuedInCombat;

		public void Serialize(PacketWriter writer)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			writer.WriteEnum<EasyRightClickableModelType>(Type);
			PacketWriterExtensions.WriteFullModelId(writer, ModelId);
			switch (Type)
			{
			case EasyRightClickableModelType.Card:
				writer.Write<NetCombatCard>(NetCombatCard);
				break;
			case EasyRightClickableModelType.Power:
				writer.WriteUInt(CreatureCombatId, 32);
				break;
			case EasyRightClickableModelType.Potion:
				writer.WriteUInt(PotionIndex, 32);
				break;
			}
			writer.Write<RightClickContext.Payload>(Extra);
			writer.WriteBool(WasEnqueuedInCombat);
		}

		public void Deserialize(PacketReader reader)
		{
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			Type = reader.ReadEnum<EasyRightClickableModelType>();
			ModelId = PacketReaderExtensions.ReadFullModelId(reader);
			switch (Type)
			{
			case EasyRightClickableModelType.Card:
				NetCombatCard = reader.Read<NetCombatCard>();
				break;
			case EasyRightClickableModelType.Power:
				CreatureCombatId = reader.ReadUInt(32);
				break;
			case EasyRightClickableModelType.Potion:
				PotionIndex = reader.ReadUInt(32);
				break;
			}
			Extra = reader.Read<RightClickContext.Payload>();
			WasEnqueuedInCombat = reader.ReadBool();
		}

		public GameAction ToGameAction(Player player)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			return (GameAction)(object)new EasyRightClickCardAction(player, ModelId, Extra, WasEnqueuedInCombat)
			{
				Type = Type,
				NetCombatCard = NetCombatCard,
				CreatureCombatId = CreatureCombatId,
				PotionIndex = PotionIndex
			};
		}
	}
}
namespace MinionLib.Powers
{
	public sealed class MinionGuardianPower : PowerModel
	{
		public override PowerType Type => (PowerType)1;

		public override PowerStackType StackType => (PowerStackType)2;

		public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
		{
			//IL_009e: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			if (((PowerModel)this).Owner.Monster is MinionModel { Position: not MinionPosition.Front })
			{
				return target;
			}
			Player petOwner = ((PowerModel)this).Owner.PetOwner;
			if (target != ((petOwner != null) ? petOwner.Creature : null))
			{
				bool flag = true;
				if (target.PetOwner == ((PowerModel)this).Owner.PetOwner && ((PowerModel)this).Owner.PetOwner != null && target.GetPower<MinionGuardianPower>() != null)
				{
					IReadOnlyList<Creature> pets = target.PetOwner.PlayerCombatState.Pets;
					if (ListExtensions.IndexOf<Creature>(pets, ((PowerModel)this).Owner) < ListExtensions.IndexOf<Creature>(pets, target))
					{
						flag = false;
					}
				}
				if (flag)
				{
					return target;
				}
			}
			if (((PowerModel)this).Owner.IsDead)
			{
				return target;
			}
			if (!((Enum)props).HasFlag((Enum)(object)(ValueProp)8) || ((Enum)props).HasFlag((Enum)(object)(ValueProp)4))
			{
				return target;
			}
			return ((PowerModel)this).Owner;
		}
	}
}
namespace MinionLib.Powers.Patches
{
	[HarmonyPatch(typeof(CreatureCmd), "GainBlock", new Type[]
	{
		typeof(Creature),
		typeof(decimal),
		typeof(ValueProp),
		typeof(CardPlay),
		typeof(bool)
	})]
	public static class MinionGuardianBlockToHpPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(Creature creature, decimal amount, ValueProp props, CardPlay? cardPlay, bool fast, ref Task<decimal> __result)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (amount <= 0m || creature.GetPower<MinionGuardianPower>() == null || creature.IsDead)
			{
				return true;
			}
			__result = GainBlock(creature, amount, props, cardPlay, fast);
			return false;
		}

		public static async Task<decimal> GainBlock(Creature creature, decimal amount, ValueProp props, CardPlay? cardPlay, bool fast = false)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (CombatManager.Instance.IsOverOrEnding)
			{
				return 0m;
			}
			ICombatState combatState = creature.CombatState;
			await Hook.BeforeBlockGained(combatState, creature, amount, props, (cardPlay != null) ? cardPlay.Card : null);
			decimal modifiedAmount = amount;
			IEnumerable<AbstractModel> enumerable = default(IEnumerable<AbstractModel>);
			modifiedAmount = Hook.ModifyBlock(combatState, creature, modifiedAmount, props, (cardPlay != null) ? cardPlay.Card : null, cardPlay, ref enumerable);
			modifiedAmount = Math.Max(modifiedAmount, 0m);
			await Hook.AfterModifyingBlockAmount(combatState, modifiedAmount, (cardPlay != null) ? cardPlay.Card : null, cardPlay, enumerable);
			if (modifiedAmount > 0m)
			{
				SfxCmd.Play("event:/sfx/block_gain", 1f);
				VfxCmd.PlayOnCreatureCenter(creature, "vfx/vfx_block");
				await CreatureCmd.SetMaxHp(creature, (decimal)creature.MaxHp + modifiedAmount);
				await CreatureCmd.Heal(creature, modifiedAmount, false);
				CombatManager.Instance.History.BlockGained(combatState, creature, (int)modifiedAmount, props, cardPlay);
				if (!fast)
				{
					await Cmd.CustomScaledWait(0.1f, 0.25f, false, default(CancellationToken));
				}
				else
				{
					await Cmd.CustomScaledWait(0f, 0.03f, false, default(CancellationToken));
				}
			}
			await Hook.AfterBlockGained(combatState, creature, modifiedAmount, props, (cardPlay != null) ? cardPlay.Card : null);
			return 0m;
		}
	}
	[HarmonyPatch(typeof(CreatureCmd), "Damage", new Type[]
	{
		typeof(PlayerChoiceContext),
		typeof(IEnumerable<Creature>),
		typeof(decimal),
		typeof(ValueProp),
		typeof(Creature),
		typeof(CardModel)
	})]
	public static class MinionGuardianOverkillPatch
	{
		private static readonly AsyncLocal<bool> IsHandling = new AsyncLocal<bool>();

		public static readonly AsyncLocal<Creature?> SuppressedOwner = new AsyncLocal<Creature>();

		[HarmonyPrefix]
		private static bool Prefix(PlayerChoiceContext choiceContext, IEnumerable<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ref Task<IEnumerable<DamageResult>> __result)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			if (IsHandling.Value)
			{
				return true;
			}
			List<Creature> list = targets.ToList();
			if (list.Count != 1)
			{
				return true;
			}
			if (!ShouldHandle(list[0], props))
			{
				return true;
			}
			__result = HandleWithOverkillRedirect(choiceContext, list, amount, props, dealer, cardSource);
			return false;
		}

		private static bool ShouldHandle(Creature target, ValueProp props)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			if (!target.IsPlayer || target.Player == null || target.IsDead || target.CombatState == null)
			{
				return false;
			}
			if (!((Enum)props).HasFlag((Enum)(object)(ValueProp)8) || ((Enum)props).HasFlag((Enum)(object)(ValueProp)4))
			{
				return false;
			}
			return target.Pets.Any((Creature p) => p.IsAlive && IsFrontGuardian(p));
		}

		private static async Task<IEnumerable<DamageResult>> HandleWithOverkillRedirect(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> targets, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			IsHandling.Value = true;
			try
			{
				Creature owner = targets[0];
				if (owner.Player == null || owner.CombatState == null)
				{
					return await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)targets, amount, props, dealer, cardSource);
				}
				List<uint> guardianOrder = (from p in PetOrderSnapshotManager.GetSnapshot(owner.Player, onlyAlive: false)
					where IsFrontGuardian(p) && p.CombatId.HasValue
					select p.CombatId.Value).ToList();
				SuppressedOwner.Value = owner;
				List<DamageResult> initialResults;
				try
				{
					initialResults = (await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)targets, amount, props, dealer, cardSource)).ToList();
				}
				finally
				{
					SuppressedOwner.Value = null;
				}
				DamageResult val = initialResults.FirstOrDefault(delegate(DamageResult r)
				{
					if (r.Receiver != owner && r.Receiver.PetOwner == owner.Player)
					{
						if (!IsFrontGuardian(r.Receiver))
						{
							uint? combatId = r.Receiver.CombatId;
							if (combatId.HasValue)
							{
								uint valueOrDefault = combatId.GetValueOrDefault();
								return guardianOrder.Contains(valueOrDefault);
							}
							return false;
						}
						return true;
					}
					return false;
				});
				if (val == null || val.OverkillDamage <= 0 || !val.Receiver.CombatId.HasValue)
				{
					return initialResults;
				}
				List<DamageResult> redirectedResults = new List<DamageResult>();
				decimal num = val.OverkillDamage;
				uint value = val.Receiver.CombatId.Value;
				ValueProp directProps = (ValueProp)(props | 4);
				int num2 = guardianOrder.IndexOf(value);
				if (num2 < 0)
				{
					if (num > 0m)
					{
						DamageResult item = (DamageResult)(((object)(await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)new <>z__ReadOnlySingleElementList<Creature>(owner), num, directProps, dealer, cardSource)).FirstOrDefault()) ?? ((object)new DamageResult(owner, directProps)));
						redirectedResults.Add(item);
					}
					initialResults.AddRange(redirectedResults);
					return initialResults;
				}
				foreach (uint item3 in guardianOrder.Skip(num2 + 1))
				{
					if (!(num <= 0m))
					{
						Creature defender = owner.CombatState.GetCreature((uint?)item3);
						if (defender != null && defender.IsAlive && IsFrontGuardian(defender))
						{
							DamageResult val2 = (DamageResult)(((object)(await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)new <>z__ReadOnlySingleElementList<Creature>(defender), num, directProps, dealer, cardSource)).FirstOrDefault()) ?? ((object)new DamageResult(defender, directProps)));
							redirectedResults.Add(val2);
							num = val2.OverkillDamage;
						}
						continue;
					}
					break;
				}
				if (num > 0m)
				{
					DamageResult item2 = (DamageResult)(((object)(await CreatureCmd.Damage(choiceContext, (IEnumerable<Creature>)new <>z__ReadOnlySingleElementList<Creature>(owner), num, directProps, dealer, cardSource)).FirstOrDefault()) ?? ((object)new DamageResult(owner, directProps)));
					redirectedResults.Add(item2);
				}
				initialResults.AddRange(redirectedResults);
				return initialResults;
			}
			finally
			{
				IsHandling.Value = false;
			}
		}

		private static bool IsFrontGuardian(Creature creature)
		{
			if (creature.GetPower<MinionGuardianPower>() != null)
			{
				if (creature.Monster is MinionModel minionModel)
				{
					return minionModel.Position == MinionPosition.Front;
				}
				return true;
			}
			return false;
		}
	}
	[HarmonyPatch(typeof(Creature), "LoseHpInternal", new Type[]
	{
		typeof(decimal),
		typeof(ValueProp)
	})]
	public static class MinionGuardianOwnerDamageSuppressPatch
	{
		[HarmonyPrefix]
		private static bool Prefix(Creature __instance, decimal amount, ValueProp props, ref DamageResult __result)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			Creature value = MinionGuardianOverkillPatch.SuppressedOwner.Value;
			if (value == null || __instance != value || amount <= 0m)
			{
				return true;
			}
			__result = new DamageResult(__instance, props);
			return false;
		}
	}
}
namespace MinionLib.Minion
{
	public abstract class MinionModel : MonsterModel
	{
		public override string DeathSfx => "event:/sfx/characters/osty/osty_die";

		public override bool HasDeathSfx => true;

		public MinionPosition Position { get; internal set; }

		protected override MonsterMoveStateMachine GenerateMoveStateMachine()
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Expected O, but got Unknown
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Expected O, but got Unknown
			MoveState val = new MoveState("MINION_IDLE", (Func<IReadOnlyList<Creature>, Task>)((IReadOnlyList<Creature> _) => Task.CompletedTask), Array.Empty<AbstractIntent>())
			{
				FollowUpState = null
			};
			val.FollowUpState = (MonsterState)(object)val;
			return new MonsterMoveStateMachine((IEnumerable<MonsterState>)new <>z__ReadOnlySingleElementList<MonsterState>((MonsterState)(object)val), (MonsterState)(object)val);
		}

		public virtual Task OnSummon(PlayerChoiceContext choiceContext, Player owner, MinionSummonOptions options)
		{
			return Task.CompletedTask;
		}
	}
	public readonly record struct MinionSummonOptions(decimal? MaxHp = null, decimal? PrimaryStatAmount = null, decimal? SecondaryStatAmount = null, decimal? TertiaryStatAmount = null, CardModel? Source = null, MinionPosition Position = MinionPosition.Front);
	public enum MinionPosition
	{
		Front,
		Back,
		FrontUpper,
		BackUpper,
		Upper
	}
}
namespace MinionLib.Minion.Patches
{
	[HarmonyPatch(typeof(NCreature), "ToggleIsInteractable")]
	public static class MinionInteractablePatch2
	{
		[HarmonyPrefix]
		private static void Prefix(NCreature __instance, ref bool on)
		{
			if (__instance.Entity.Monster is MinionModel && LocalContext.IsMe(__instance.Entity.PetOwner))
			{
				on = true;
			}
		}
	}
	[HarmonyPatch(typeof(NCombatRoom), "AddCreature")]
	public static class MinionInteractablePatch
	{
		[HarmonyPrefix]
		private static bool Prefix(NCombatRoom __instance, out IReadOnlyList<MinionNodePosition> __state)
		{
			__state = MinionLayoutManager.GetCurrentMinionPositions(__instance);
			return true;
		}

		[HarmonyPostfix]
		private static void Postfix(NCombatRoom __instance, Creature creature, IReadOnlyList<MinionNodePosition> __state)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			MinionAnimCmd.InstantMove(__state);
			if (creature.PetOwner != null && creature.Monster is MinionModel)
			{
				((Control)__instance.GetCreatureNode(creature)).Position = ((Control)__instance.GetCreatureNode(creature.PetOwner.Creature)).Position;
			}
		}
	}
	[HarmonyPatch(typeof(CreatureCmd), "KillWithoutCheckingWinCondition")]
	public static class MinionKillPatch
	{
		[HarmonyPostfix]
		private static void Postfix(ref Task __result, Creature creature)
		{
			__result = AwaitAndCleanupAsync(__result, creature);
		}

		private static async Task AwaitAndCleanupAsync(Task originalTask, Creature creature)
		{
			await originalTask;
			if (creature == null || (int)creature.Side != 1 || !creature.IsPet || !(creature.Monster is MinionModel) || creature.CombatState == null)
			{
				return;
			}
			ICombatState combatState = creature.CombatState;
			if (combatState != null && Hook.ShouldCreatureBeRemovedFromCombatAfterDeath(combatState, creature))
			{
				CombatManager instance = CombatManager.Instance;
				if (instance != null)
				{
					instance.RemoveCreature(creature);
				}
				if (combatState != null)
				{
					combatState.RemoveCreature(creature, true);
				}
			}
		}
	}
}
namespace MinionLib.Layout
{
	public class DefaultMinionLayout : IMinionLayout
	{
		public static readonly Vector2 MinionSize;

		public bool IsActive => true;

		public void ApplyLayout(MinionLayoutContext context)
		{
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			List<NCreature> list = context.UnhandledMinions.ToList();
			if (list.Count == 0)
			{
				return;
			}
			foreach (MinionNodePosition item in CalculateMinionPositions(context.Room, list))
			{
				context.Positions[item.Node] = item.Position;
			}
		}

		public static IReadOnlyList<Vector2> GenerateGridPoints(MinionPosition position, int count)
		{
			if (count <= 0)
			{
				return Array.Empty<Vector2>();
			}
			if (position == MinionPosition.Upper)
			{
				_ = count / 2;
				return Enumerable.Range(1, count).Select((Func<int, Vector2>)delegate(int i)
				{
					//IL_002c: Unknown result type (might be due to invalid IL or missing references)
					return new Vector2((i == count && count % 2 == 1) ? 0f : (-0.5f + (float)(i % 2)), (float)(-(i - 1) / 2));
				}).ToList();
			}
			bool flag = ((position == MinionPosition.Front || position == MinionPosition.FrontUpper) ? true : false);
			bool flag2 = flag;
			float num = (flag2 ? 3f : 1.5f);
			float num2 = (flag2 ? 0.75f : 0.25f);
			float num3 = (float)(count + 1) * 0.5f;
			float num4 = ((num3 <= num) ? num3 : (num + num2 * MathF.Log((num3 - num) / num2 + 1f)));
			float last = 0f;
			float first = (flag2 ? num4 : (0f - num4));
			return Enumerable.Range(2, count).Select((Func<int, Vector2>)delegate(int i)
			{
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				return new Vector2(float.Lerp(first, last, (float)i / (float)(count + 1)), (float)(-i % 2));
			}).ToList();
		}

		public static IReadOnlyList<OwnerWithMinionsNodes> GetMinionOwnerNodePairs(NCombatRoom room, IEnumerable<NCreature> unhandledMinions)
		{
			return (from c in unhandledMinions
				group c by c.Entity.PetOwner).Select(delegate(IGrouping<Player, NCreature> g)
			{
				Player key = g.Key;
				List<NCreature> minions = Enumerable.Select(selector: (Func<Creature, NCreature>)((IReadOnlyDictionary<Creature, NCreature>)g.ToDictionary((NCreature c) => c.Entity, (NCreature c) => c)).GetValueOrDefault<Creature, NCreature>, source: (IEnumerable<Creature>)key.PlayerCombatState.Pets).OfType<NCreature>().ToList();
				return new OwnerWithMinionsNodes(room.GetCreatureNode(g.Key.Creature), minions);
			}).ToList();
		}

		public static Vector2 CalculateBaseOffset(MinionPosition minionPosition, ILookup<MinionPosition, NCreature> lookup)
		{
			//IL_011f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0125: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			switch (minionPosition)
			{
			case MinionPosition.Front:
				if (lookup.Contains(MinionPosition.FrontUpper) && lookup[MinionPosition.Front].Count() >= 2)
				{
					return new Vector2(200f, 50f);
				}
				return new Vector2(200f, 0f);
			case MinionPosition.Back:
				if (lookup.Contains(MinionPosition.BackUpper) && lookup[MinionPosition.Back].Count() >= 2)
				{
					return new Vector2(-200f, 50f);
				}
				return new Vector2(-200f, 0f);
			case MinionPosition.FrontUpper:
				if (lookup[MinionPosition.Upper].Count() <= 2)
				{
					return new Vector2(100f + 50f * (float)lookup[MinionPosition.Upper].Count(), -350f);
				}
				return new Vector2(200f, -350f);
			case MinionPosition.BackUpper:
				if (lookup[MinionPosition.Upper].Count() <= 2)
				{
					return new Vector2(-100f - 50f * (float)lookup[MinionPosition.Upper].Count(), -350f);
				}
				return new Vector2(-200f, -350f);
			case MinionPosition.Upper:
				return new Vector2(0f, -450f);
			default:
				return Vector2.Zero;
			}
		}

		public static IReadOnlyList<MinionNodePosition> CalculateMinionPositions(NCombatRoom room, IEnumerable<NCreature> unhandledMinions)
		{
			return GetMinionOwnerNodePairs(room, unhandledMinions).SelectMany(delegate(OwnerWithMinionsNodes pair)
			{
				OwnerWithMinionsNodes ownerWithMinionsNodes = pair;
				var (ownerNode, source) = ownerWithMinionsNodes;
				ILookup<MinionPosition, NCreature> grouped = source.ToLookup((NCreature c) => ((MinionModel)(object)c.Entity.Monster).Position);
				return grouped.SelectMany(delegate(IGrouping<MinionPosition, NCreature> g)
				{
					//IL_001c: Unknown result type (might be due to invalid IL or missing references)
					//IL_0021: Unknown result type (might be due to invalid IL or missing references)
					MinionPosition key = g.Key;
					Vector2 offset = CalculateBaseOffset(key, grouped);
					IEnumerable<Vector2> second = GenerateGridPoints(key, g.Count()).Select(delegate(Vector2 v)
					{
						//IL_0000: Unknown result type (might be due to invalid IL or missing references)
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_0006: Unknown result type (might be due to invalid IL or missing references)
						//IL_000c: Unknown result type (might be due to invalid IL or missing references)
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0021: Unknown result type (might be due to invalid IL or missing references)
						//IL_0026: Unknown result type (might be due to invalid IL or missing references)
						return v * MinionSize + offset + ((Control)ownerNode).Position;
					});
					return g.Zip(second, delegate(NCreature node, Vector2 position)
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						return new MinionNodePosition(node, position);
					});
				});
			}).ToList();
		}

		static DefaultMinionLayout()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			MinionSize = new Vector2(150f, 200f);
		}
	}
	public interface IMinionLayout
	{
		bool IsActive { get; }

		void ApplyLayout(MinionLayoutContext context);
	}
	public readonly record struct OwnerWithMinionsNodes(NCreature Owner, IReadOnlyList<NCreature> Minions);
	public readonly record struct MinionNodePosition(NCreature Node, Vector2 Position);
	public class MinionLayoutContext
	{
		public NCombatRoom Room { get; }

		public IReadOnlyList<NCreature> AllMinions { get; }

		public Dictionary<NCreature, Vector2> Positions { get; }

		public IEnumerable<NCreature> UnhandledMinions => AllMinions.Where((NCreature m) => !Positions.ContainsKey(m));

		public MinionLayoutContext(NCombatRoom room)
		{
			Room = room;
			AllMinions = room.CreatureNodes.Where((NCreature n) => n.IsMinionNode()).ToList();
			Positions = new Dictionary<NCreature, Vector2>();
		}
	}
	public static class MinionLayoutManager
	{
		private static readonly List<(IMinionLayout layout, int priority, int order)> LayoutsWithPriority;

		private static int _counter;

		public static IEnumerable<IMinionLayout> Layouts => LayoutsWithPriority.Select<(IMinionLayout, int, int), IMinionLayout>(((IMinionLayout layout, int priority, int order) x) => x.layout);

		static MinionLayoutManager()
		{
			LayoutsWithPriority = new List<(IMinionLayout, int, int)>();
			Register(new DefaultMinionLayout());
		}

		public static void Register(IMinionLayout layout, int priority = 0)
		{
			LayoutsWithPriority.Add((layout, priority, _counter++));
			LayoutsWithPriority.Sort(delegate((IMinionLayout layout, int priority, int order) a, (IMinionLayout layout, int priority, int order) b)
			{
				int num = b.priority.CompareTo(a.priority);
				return (num == 0) ? b.order.CompareTo(a.order) : num;
			});
		}

		public static IEnumerable<MinionNodePosition> CalculateLayout(NCombatRoom room)
		{
			MinionLayoutContext minionLayoutContext = new MinionLayoutContext(room);
			foreach (IMinionLayout layout in Layouts)
			{
				if (layout.IsActive)
				{
					layout.ApplyLayout(minionLayoutContext);
				}
			}
			return minionLayoutContext.Positions.Select<KeyValuePair<NCreature, Vector2>, MinionNodePosition>(delegate(KeyValuePair<NCreature, Vector2> entry)
			{
				//IL_0009: Unknown result type (might be due to invalid IL or missing references)
				return new MinionNodePosition(entry.Key, entry.Value);
			}).ToList();
		}

		public static IReadOnlyList<MinionNodePosition> GetCurrentMinionPositions(NCombatRoom room)
		{
			return room.CreatureNodes.Where((NCreature n) => n.IsMinionNode()).Select(delegate(NCreature c)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				return new MinionNodePosition(c, ((Control)c).Position);
			}).ToList();
		}
	}
	public static class NCreatureExtensions
	{
		public static bool IsMinionNode(this NCreature node)
		{
			Creature entity = node.Entity;
			if (entity != null && entity.Monster is MinionModel && entity.IsAlive)
			{
				return entity.PetOwner != null;
			}
			return false;
		}
	}
}
namespace MinionLib.Initialization
{
	public static class MinionHookInitializer
	{
		public static void Initialize()
		{
			CombatManager.Instance.TurnStarted += OnTurnStarted;
			CombatManager.Instance.TurnEnded += OnTurnEnded;
			CombatManager.Instance.CombatSetUp += OnCombatSetUp;
			CombatManager.Instance.CombatEnded += OnCombatEnded;
		}

		public static void Deinitialize()
		{
			CombatManager.Instance.TurnStarted -= OnTurnStarted;
			CombatManager.Instance.TurnEnded -= OnTurnEnded;
			CombatManager.Instance.CombatSetUp -= OnCombatSetUp;
			CombatManager.Instance.CombatEnded -= OnCombatEnded;
		}

		private static void OnTurnStarted(CombatState combatState)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Invalid comparison between Unknown and I4
			if ((int)combatState.CurrentSide == 1)
			{
				MinionAnimCmd.Rearrange();
			}
		}

		private static void OnTurnEnded(CombatState combatState)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			CreatureActionQueueThreshold.Clear();
			if ((int)combatState.CurrentSide == 2)
			{
				MinionAnimCmd.Rearrange();
			}
		}

		private static void OnCombatSetUp(CombatState combatState)
		{
			CreatureActionQueueThreshold.Clear();
			PetOrderSnapshotManager.ClearAllSnapshots();
		}

		private static void OnCombatEnded(CombatRoom combatRoom)
		{
			CreatureActionQueueThreshold.Clear();
			PetOrderSnapshotManager.ClearAllSnapshots();
		}
	}
}
namespace MinionLib.Component
{
	public abstract class CardComponent : ICardComponent, IGeneratedBinarySerializable
	{
		[CompilerGenerated]
		private DynamicVarSet <DynamicVars>k__BackingField;

		protected virtual IEnumerable<DynamicVar> SmartVars => Array.Empty<DynamicVar>();

		protected virtual IEnumerable<DynamicVar> ExtraVars => Array.Empty<DynamicVar>();

		protected virtual LocString PrefixLocString
		{
			get
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				return new LocString("cards", ComponentId + ".prefix");
			}
		}

		protected virtual LocString PostfixLocString
		{
			get
			{
				//IL_0015: Unknown result type (might be due to invalid IL or missing references)
				//IL_001b: Expected O, but got Unknown
				return new LocString("cards", ComponentId + ".postfix");
			}
		}

		public abstract string ComponentId { get; }

		public IComponentsCardModel? ComponentsCard { get; private set; }

		public CardModel? Card
		{
			get
			{
				IComponentsCardModel? componentsCard = ComponentsCard;
				return (CardModel?)((componentsCard is CardModel) ? componentsCard : null);
			}
		}

		public DynamicVarSet DynamicVars
		{
			get
			{
				//IL_0021: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Expected O, but got Unknown
				if (<DynamicVars>k__BackingField != null)
				{
					return <DynamicVars>k__BackingField;
				}
				<DynamicVars>k__BackingField = new DynamicVarSet(SmartVars.Concat(ExtraVars));
				return <DynamicVars>k__BackingField;
			}
		}

		public virtual bool ShouldGlowGoldInternal => false;

		public virtual bool ShouldGlowRedInternal => false;

		public virtual Color? GlowColor => null;

		public virtual TargetType? ExtraTargetType => null;

		public virtual CardType? CardTypeOverride => null;

		public virtual CardRarity? CardRarityOverride => null;

		public IEnumerable<CardTag> ExtraTags => Array.Empty<CardTag>();

		public virtual bool IsPlayable => true;

		public virtual bool HasTurnEndInHandEffect => false;

		public virtual IEnumerable<IHoverTip> HoverTips => Array.Empty<IHoverTip>();

		public void Attach(IComponentsCardModel card, bool isInternal = false)
		{
			ComponentsCard = card;
			if (!isInternal)
			{
				OnAttach();
			}
		}

		public void Detach(bool isInternal = false)
		{
			if (!isInternal)
			{
				OnDetach();
			}
			ComponentsCard = null;
		}

		public virtual ICardComponent DeepClone()
		{
			return CardComponentStateSerializer.DeepClone(this);
		}

		public virtual bool TryMergeWith(ICardComponent incoming, ApplyComponentOptions options, out ICardComponent? merged)
		{
			merged = null;
			return false;
		}

		public virtual bool TrySubtractiveMergeWith(ICardComponent incoming, ApplyComponentOptions options, out ICardComponent? merged)
		{
			merged = null;
			return false;
		}

		public virtual void Serialize(ArrayBufferWriter<byte> writer)
		{
		}

		public virtual bool Deserialize(ref ReadOnlySpan<byte> reader)
		{
			return true;
		}

		public virtual PileType? GetResultPileTypeForCardPlay()
		{
			return null;
		}

		public virtual string GetFormattedPrefix(Dictionary<string, object> argsFromCard)
		{
			LocString prefixLocString = PrefixLocString;
			if (!prefixLocString.Exists())
			{
				return "";
			}
			foreach (var (text2, obj2) in argsFromCard)
			{
				prefixLocString.AddObj(text2, obj2);
			}
			SmartAddArgs(prefixLocString);
			return FormatPrefix(prefixLocString);
		}

		public virtual string GetFormattedPostfix(Dictionary<string, object> argsFromCard)
		{
			LocString postfixLocString = PostfixLocString;
			if (!postfixLocString.Exists())
			{
				return "";
			}
			foreach (var (text2, obj2) in argsFromCard)
			{
				postfixLocString.AddObj(text2, obj2);
			}
			SmartAddArgs(postfixLocString);
			return FormatPostfix(postfixLocString);
		}

		public virtual bool CanHandleRightClickLocal(RightClickContext context)
		{
			return CanHandleRightClick(context);
		}

		public virtual bool CanHandleRightClick(RightClickContext context)
		{
			return false;
		}

		public virtual Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext)
		{
			return Task.CompletedTask;
		}

		public virtual void OnUpgrade(ComponentContext componentContext)
		{
		}

		public virtual void AfterDowngraded(ComponentContext componentContext)
		{
		}

		protected virtual void OnAttach()
		{
		}

		protected virtual void OnDetach()
		{
		}

		protected virtual void SmartAddArgs(LocString loc)
		{
			DynamicVars.AddTo(loc);
			string colorPrefix = (string)loc.Variables["energyPrefix"];
			foreach (KeyValuePair<string, object> variable in loc.Variables)
			{
				variable.Deconstruct(out var _, out var value);
				EnergyVar val = (EnergyVar)((value is EnergyVar) ? value : null);
				if (val != null)
				{
					val.ColorPrefix = colorPrefix;
				}
			}
		}

		protected virtual string FormatPrefix(LocString loc)
		{
			return loc.GetFormattedText();
		}

		protected virtual string FormatPostfix(LocString loc)
		{
			return loc.GetFormattedText();
		}

		public virtual Task OnPlayPrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task OnPlayPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task OnEnqueuePlayVfxPrefix(Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task OnEnqueuePlayVfxPostfix(Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task OnTurnEndInHandPrefix(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task OnTurnEndInHandPostfix(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCardPlayedPrefix(CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCardPlayedPostfix(CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardPlayedPrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardPlayedPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardPlayedLatePrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardPlayedLatePostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPlayerTurnStartEarlyPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPlayerTurnStartEarlyPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPlayerTurnStartPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPlayerTurnStartPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPlayerTurnStartLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPlayerTurnStartLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPostPlayPhaseEnteredPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPostPlayPhaseEnteredPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPrePlayPhaseEnteredEarlyPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPrePlayPhaseEnteredEarlyPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPrePlayPhaseEnteredPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPrePlayPhaseEnteredPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPrePlayPhaseEnteredLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAutoPrePlayPhaseEnteredLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnEndVeryEarlyPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnEndVeryEarlyPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnEndEarlyPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnEndEarlyPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnEndPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnEndPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnEndPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnEndPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnEndLatePrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnEndLatePostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterActEnteredPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterActEnteredPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAddToDeckPreventedPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAddToDeckPreventedPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeAttackPrefix(AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeAttackPostfix(AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAttackPrefix(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterAttackPostfix(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterBlockClearedPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterBlockClearedPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeBlockGainedPrefix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeBlockGainedPostfix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterBlockGainedPrefix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterBlockGainedPostfix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterBlockBrokenPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterBlockBrokenPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardChangedPilesPrefix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardChangedPilesPostfix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardChangedPilesLatePrefix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardChangedPilesLatePostfix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardDiscardedPrefix(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardDiscardedPostfix(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardDrawnEarlyPrefix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardDrawnEarlyPostfix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardDrawnPrefix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardDrawnPostfix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardEnteredCombatPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardEnteredCombatPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardGeneratedForCombatPrefix(CardModel card, Player? creator, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardGeneratedForCombatPostfix(CardModel card, Player? creator, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardExhaustedPrefix(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCardExhaustedPostfix(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCardAutoPlayedPrefix(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCardAutoPlayedPostfix(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCombatStartPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCombatStartPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCombatStartLatePrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCombatStartLatePostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCombatEndPrefix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCombatEndPostfix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCombatVictoryEarlyPrefix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCombatVictoryEarlyPostfix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCombatVictoryPrefix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCombatVictoryPostfix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCreatureAddedToCombatPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCreatureAddedToCombatPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCurrentHpChangedPrefix(Creature creature, decimal delta, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterCurrentHpChangedPostfix(Creature creature, decimal delta, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDamageGivenPrefix(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDamageGivenPostfix(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeDamageReceivedPrefix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeDamageReceivedPostfix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDamageReceivedPrefix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDamageReceivedPostfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDamageReceivedLatePrefix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDamageReceivedLatePostfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeDeathPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeDeathPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDeathPrefix(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDeathPostfix(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDiedToDoomPrefix(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterDiedToDoomPostfix(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterEnergyResetPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterEnergyResetPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterEnergyResetLatePrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterEnergyResetLatePostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterEnergySpentPrefix(CardModel card, int amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterEnergySpentPostfix(CardModel card, int amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCardRemovedPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeCardRemovedPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeFlushPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeFlushPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeFlushLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeFlushLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterFlushPrefix(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterFlushPostfix(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterGoldGainedPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterGoldGainedPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeHandDrawPrefix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeHandDrawPostfix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeHandDrawLatePrefix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeHandDrawLatePostfix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterHandEmptiedPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterHandEmptiedPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterItemPurchasedPrefix(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterItemPurchasedPostfix(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterMapGeneratedPrefix(ActMap map, int actIndex, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterMapGeneratedPostfix(ActMap map, int actIndex, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingBlockAmountPrefix(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingBlockAmountPostfix(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingCardPlayCountPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingCardPlayCountPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingCardPlayResultPileOrPositionPrefix(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingCardPlayResultPileOrPositionPostfix(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingOrbPassiveTriggerCountPrefix(OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingOrbPassiveTriggerCountPostfix(OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingCardRewardOptionsPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingCardRewardOptionsPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingDamageAmountPrefix(CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingDamageAmountPostfix(CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingEnergyGainPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingEnergyGainPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingHandDrawPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingHandDrawPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPreventingDrawPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPreventingDrawPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingHpLostBeforeOstyPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingHpLostBeforeOstyPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingHpLostAfterOstyPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingHpLostAfterOstyPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingPowerAmountReceivedPrefix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingPowerAmountReceivedPostfix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingPowerAmountGivenPrefix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingPowerAmountGivenPostfix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingRewardsPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingRewardsPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterOrbChanneledPrefix(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterOrbChanneledPostfix(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterOrbEvokedPrefix(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterOrbEvokedPostfix(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterOstyRevivedPrefix(Creature osty, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterOstyRevivedPostfix(Creature osty, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforePotionUsedPrefix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforePotionUsedPostfix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPotionUsedPrefix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPotionUsedPostfix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPotionDiscardedPrefix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPotionDiscardedPostfix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPotionProcuredPrefix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPotionProcuredPostfix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforePowerAmountChangedPrefix(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforePowerAmountChangedPostfix(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPowerAmountChangedPrefix(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPowerAmountChangedPostfix(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPreventingBlockClearPrefix(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPreventingBlockClearPostfix(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPreventingDeathPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterPreventingDeathPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRestSiteHealPrefix(Player player, bool isMimicked, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRestSiteHealPostfix(Player player, bool isMimicked, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRestSiteSmithPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRestSiteSmithPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRewardTakenPrefix(Player player, Reward reward, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRewardTakenPostfix(Player player, Reward reward, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeRoomEnteredPrefix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeRoomEnteredPostfix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRoomEnteredPrefix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterRoomEnteredPostfix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterShufflePrefix(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterShufflePostfix(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterStarsSpentPrefix(int amount, Player spender, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterStarsSpentPostfix(int amount, Player spender, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterStarsGainedPrefix(int amount, Player gainer, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterStarsGainedPostfix(int amount, Player gainer, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterForgePrefix(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterForgePostfix(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSummonPrefix(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSummonPostfix(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterTakingExtraTurnPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterTakingExtraTurnPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterTargetingBlockedVfxPrefix(Creature blocker, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterTargetingBlockedVfxPostfix(Creature blocker, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnStartPrefix(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task BeforeSideTurnStartPostfix(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnStartPrefix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnStartPostfix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnStartLatePrefix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterSideTurnStartLatePostfix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingGoldGainedPrefix(Player player, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual Task AfterModifyingGoldGainedPostfix(Player player, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		public virtual void AfterTransformedFromPrefix(ComponentContext componentContext)
		{
		}

		public virtual void AfterTransformedFromPostfix(ComponentContext componentContext)
		{
		}

		public virtual void AfterTransformedToPrefix(ComponentContext componentContext)
		{
		}

		public virtual void AfterTransformedToPostfix(ComponentContext componentContext)
		{
		}

		public virtual int ModifyAttackHitCount(AttackCommand attack, int hitCount)
		{
			return hitCount;
		}

		public virtual decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			return 0m;
		}

		public virtual decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			return 1m;
		}

		public virtual int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			return playCount;
		}

		public virtual (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return (pileType, position);
		}

		public virtual int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
		{
			return triggerCount;
		}

		public virtual CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
		{
			return options;
		}

		public virtual CardCreationOptions ModifyCardRewardCreationOptionsLate(Player player, CardCreationOptions options)
		{
			return options;
		}

		public virtual decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
		{
			return odds;
		}

		public virtual decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return 0m;
		}

		public virtual decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return decimal.MaxValue;
		}

		public virtual decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return 1m;
		}

		public virtual decimal ModifyEnergyGain(Player player, decimal amount)
		{
			return amount;
		}

		public virtual decimal ModifyGoldGained(Player player, decimal amount)
		{
			return amount;
		}

		public virtual ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
		{
			return map;
		}

		public virtual ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
		{
			return map;
		}

		public virtual decimal ModifyHandDraw(Player player, decimal count)
		{
			return count;
		}

		public virtual decimal ModifyHandDrawLate(Player player, decimal count)
		{
			return count;
		}

		public virtual decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		public virtual decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		public virtual decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		public virtual decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		public virtual decimal ModifyMaxEnergy(Player player, decimal amount)
		{
			return amount;
		}

		public virtual IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
		{
			return options;
		}

		public virtual CardRarity ModifyMerchantCardRarity(Player player, CardRarity rarity)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return rarity;
		}

		public virtual void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
		{
		}

		public virtual decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
		{
			return cost;
		}

		public virtual decimal ModifyOrbValue(OrbModel orb, decimal value)
		{
			return value;
		}

		public virtual decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			return 0m;
		}

		public virtual decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			return 1m;
		}

		public virtual decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
		{
			return amount;
		}

		public virtual void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
		{
		}

		public virtual decimal ModifySummonAmount(Player summoner, decimal amount, AbstractModel? source)
		{
			return amount;
		}

		public virtual Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
		{
			return target;
		}

		public virtual EventModel ModifyNextEvent(EventModel currentEvent)
		{
			return currentEvent;
		}

		public virtual IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
		{
			return roomTypes;
		}

		public virtual float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
		{
			return oddsIncrease;
		}

		public virtual int ModifyXValue(CardModel card, int originalValue)
		{
			return originalValue;
		}

		public virtual bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
		{
			newCard = null;
			return false;
		}

		public virtual bool TryModifyCardBeingAddedToDeckLate(CardModel card, out CardModel? newCard)
		{
			newCard = null;
			return false;
		}

		public virtual bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
		{
			return false;
		}

		public virtual bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			return false;
		}

		public virtual bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			return false;
		}

		public virtual bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			return false;
		}

		public virtual bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			return false;
		}

		public virtual bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
		{
			modifiedAmount = amount;
			return false;
		}

		public virtual bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
		{
			return false;
		}

		public virtual bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
		{
			return false;
		}

		public virtual bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			return false;
		}

		public virtual bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			return false;
		}

		public virtual IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
		{
			return currentExtraText;
		}

		public virtual bool TryModifyEnergyCostInCombatLate(CardModel card, decimal currentCost, out decimal modifiedCost)
		{
			modifiedCost = currentCost;
			return false;
		}

		public virtual bool ShouldAddToDeck(CardModel card)
		{
			return true;
		}

		public virtual bool ShouldAfflict(CardModel card, AfflictionModel affliction)
		{
			return true;
		}

		public virtual bool ShouldAllowAncient(Player player, AncientEventModel ancient)
		{
			return true;
		}

		public virtual bool ShouldAllowHitting(Creature creature)
		{
			return true;
		}

		public virtual bool ShouldAllowTargeting(Creature target)
		{
			return true;
		}

		public virtual bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
		{
			return false;
		}

		public virtual bool ShouldClearBlock(Creature creature)
		{
			return true;
		}

		public virtual bool ShouldDie(Creature creature)
		{
			return true;
		}

		public virtual bool ShouldDieLate(Creature creature)
		{
			return true;
		}

		public virtual bool ShouldDisableRemainingRestSiteOptions(Player player)
		{
			return true;
		}

		public virtual bool ShouldDraw(Player player, bool fromHandDraw)
		{
			return true;
		}

		public virtual bool ShouldEtherealTrigger(CardModel card)
		{
			return true;
		}

		public virtual bool ShouldFlush(Player player)
		{
			return true;
		}

		public virtual bool ShouldGainStars(decimal amount, Player player)
		{
			return true;
		}

		public virtual bool ShouldGenerateTreasure(Player player)
		{
			return true;
		}

		public virtual bool ShouldPayExcessEnergyCostWithStars(Player player)
		{
			return false;
		}

		public virtual bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
		{
			return true;
		}

		public virtual bool ShouldPlayerResetEnergy(Player player)
		{
			return true;
		}

		public virtual bool ShouldProceedToNextMapPoint()
		{
			return true;
		}

		public virtual bool ShouldProcurePotion(PotionModel potion, Player player)
		{
			return true;
		}

		public virtual bool ShouldPowerBeRemovedOnDeath(PowerModel power)
		{
			return true;
		}

		public virtual bool ShouldRefillMerchantEntry(MerchantEntry entry, Player player)
		{
			return false;
		}

		public virtual bool ShouldAllowMerchantCardRemoval(Player player)
		{
			return true;
		}

		public virtual bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
		{
			return true;
		}

		public virtual bool ShouldStopCombatFromEnding()
		{
			return false;
		}

		public virtual bool ShouldTakeExtraTurn(Player player)
		{
			return false;
		}

		public virtual bool ShouldForcePotionReward(Player player, RoomType roomType)
		{
			return false;
		}

		public virtual bool ShouldAllowFreeTravel()
		{
			return false;
		}
	}
	public static class ComponentExtensions
	{
		private static readonly FieldRef<DynamicVar, bool> WasJustUpgradedRef = AccessTools.FieldRefAccess<DynamicVar, bool>("<WasJustUpgraded>k__BackingField");

		public static void SetWasJustUpgraded(this DynamicVar var, bool value = true)
		{
			WasJustUpgradedRef.Invoke(var) = value;
		}
	}
	public abstract class ComponentsCardModel : CardModel, IComponentsCardModel, IEasyRightClickableCard, IEasyRightClickableModel, IBetterAddExtraArgsCard, ICustomGlowColorCard, IDescriptionPostProcessCard
	{
		private static readonly int MaxPhaseTransitions = 64;

		private List<ICardComponent>? _components;

		private int[] _componentStateBlob;

		[SavedProperty(/*Could not decode attribute arguments.*/)]
		public int[] MinionLibComponentStateBlob
		{
			get
			{
				if (_components != null)
				{
					_componentStateBlob = CardComponentStateSerializer.Serialize(_components);
				}
				return _componentStateBlob;
			}
			set
			{
				_componentStateBlob = value;
				_components = null;
			}
		}

		protected virtual IEnumerable<ICardComponent> CanonicalComponents => Array.Empty<ICardComponent>();

		protected sealed override bool ShouldGlowGoldInternal
		{
			get
			{
				List<ICardComponent>? components = _components;
				if (components == null || !components.Any((ICardComponent c) => c.ShouldGlowGoldInternal))
				{
					return ShouldGlowGoldInternalC;
				}
				return true;
			}
		}

		protected virtual bool ShouldGlowGoldInternalC => false;

		protected sealed override bool ShouldGlowRedInternal
		{
			get
			{
				List<ICardComponent>? components = _components;
				if (components == null || !components.Any((ICardComponent c) => c.ShouldGlowRedInternal))
				{
					return ShouldGlowRedInternalC;
				}
				return true;
			}
		}

		protected virtual bool ShouldGlowRedInternalC => false;

		protected virtual Color? GlowColorC => null;

		public sealed override CardType Type
		{
			get
			{
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				return (CardType)(((??)_components?.Select((ICardComponent c) => c.CardTypeOverride).FirstOrDefault((CardType? t) => t.HasValue)) ?? TypeC);
			}
		}

		protected virtual CardType TypeC
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return ((CardModel)this).Type;
			}
		}

		public sealed override CardRarity Rarity
		{
			get
			{
				//IL_0070: Unknown result type (might be due to invalid IL or missing references)
				//IL_0068: Unknown result type (might be due to invalid IL or missing references)
				return (CardRarity)(((??)_components?.Select((ICardComponent c) => c.CardRarityOverride).FirstOrDefault((CardRarity? r) => r.HasValue)) ?? RarityC);
			}
		}

		protected virtual CardRarity RarityC
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return ((CardModel)this).Rarity;
			}
		}

		public sealed override TargetType TargetType
		{
			get
			{
				//IL_0037: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				return SingleTargetTypesUnionManager.GetWithBase(_components?.Select((ICardComponent c) => c.ExtraTargetType).OfType<TargetType>().Append(TargetTypeC) ?? Array.Empty<TargetType>(), TargetTypeC);
			}
		}

		protected virtual TargetType TargetTypeC
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return ((CardModel)this).TargetType;
			}
		}

		public sealed override IEnumerable<CardTag> Tags => TagsC.Concat(_components?.SelectMany((ICardComponent c) => c.ExtraTags) ?? Array.Empty<CardTag>()).Distinct();

		protected virtual IEnumerable<CardTag> TagsC => ((CardModel)this).Tags;

		protected sealed override bool IsPlayable
		{
			get
			{
				List<ICardComponent>? components = _components;
				if (components == null || components.All((ICardComponent c) => c.IsPlayable))
				{
					return IsPlayableC;
				}
				return false;
			}
		}

		protected virtual bool IsPlayableC => true;

		public sealed override bool HasTurnEndInHandEffect
		{
			get
			{
				List<ICardComponent>? components = _components;
				if (components == null || !components.Any((ICardComponent c) => c.HasTurnEndInHandEffect))
				{
					return HasTurnEndInHandEffectC;
				}
				return true;
			}
		}

		protected virtual bool HasTurnEndInHandEffectC => false;

		protected sealed override IEnumerable<IHoverTip> ExtraHoverTips => _components?.SelectMany((ICardComponent c) => c.HoverTips).Concat(ExtraHoverTipsC) ?? ExtraHoverTipsC;

		protected virtual IEnumerable<IHoverTip> ExtraHoverTipsC => Array.Empty<IHoverTip>();

		public IReadOnlyList<ICardComponent> Components
		{
			get
			{
				EnsureComponentsInitialized();
				return _components;
			}
		}

		public Color? GlowColor => _components?.Select((ICardComponent c) => c.GlowColor).FirstOrDefault((Color? c) => c.HasValue) ?? GlowColorC;

		protected virtual bool ShouldAddToDeckC => true;

		protected virtual bool ShouldAfflictC => true;

		protected virtual bool ShouldAllowAncientC => true;

		protected virtual bool ShouldAllowHittingC => true;

		protected virtual bool ShouldAllowTargetingC => true;

		protected virtual bool ShouldAllowSelectingMoreCardRewardsC => false;

		protected virtual bool ShouldClearBlockC => true;

		protected virtual bool ShouldDieC => true;

		protected virtual bool ShouldDieLateC => true;

		protected virtual bool ShouldDisableRemainingRestSiteOptionsC => true;

		protected virtual bool ShouldDrawC => true;

		protected virtual bool ShouldEtherealTriggerC => true;

		protected virtual bool ShouldFlushC => true;

		protected virtual bool ShouldGainStarsC => true;

		protected virtual bool ShouldGenerateTreasureC => true;

		protected virtual bool ShouldPayExcessEnergyCostWithStarsC => false;

		protected virtual bool ShouldPlayC => true;

		protected virtual bool ShouldPlayerResetEnergyC => true;

		protected virtual bool ShouldProceedToNextMapPointC => true;

		protected virtual bool ShouldProcurePotionC => true;

		protected virtual bool ShouldPowerBeRemovedOnDeathC => true;

		protected virtual bool ShouldRefillMerchantEntryC => false;

		protected virtual bool ShouldAllowMerchantCardRemovalC => true;

		protected virtual bool ShouldCreatureBeRemovedFromCombatAfterDeathC => true;

		protected virtual bool ShouldStopCombatFromEndingC => false;

		protected virtual bool ShouldTakeExtraTurnC => false;

		protected virtual bool ShouldForcePotionRewardC => false;

		protected virtual bool ShouldAllowFreeTravelC => false;

		protected ComponentsCardModel(int canonicalEnergyCost, CardType type, CardRarity rarity, TargetType targetType, bool shouldShowInCardLibrary = true)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			_componentStateBlob = Array.Empty<int>();
			((CardModel)this)..ctor(canonicalEnergyCost, type, rarity, targetType, shouldShowInCardLibrary);
		}

		public virtual void BetterAddExtraArgsToDescription(LocString description, PileType pileType, DescriptionPreviewType previewType, Creature? target = null)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			Dictionary<string, object> source = GenerateCommonExtraArgsForComponents(pileType, previewType, target);
			StringBuilder stringBuilder = new StringBuilder();
			StringBuilder stringBuilder2 = new StringBuilder();
			int count = _components.Count;
			for (int i = 0; i < count; i++)
			{
				ICardComponent cardComponent = _components[i];
				Dictionary<string, object> argsFromCard = source.ToDictionary();
				string formattedPrefix = cardComponent.GetFormattedPrefix(argsFromCard);
				stringBuilder.Append(formattedPrefix).Append('\n');
			}
			for (int j = 0; j < count; j++)
			{
				ICardComponent cardComponent2 = _components[count - 1 - j];
				Dictionary<string, object> argsFromCard2 = source.ToDictionary();
				string formattedPostfix = cardComponent2.GetFormattedPostfix(argsFromCard2);
				stringBuilder2.Append('\n').Append(formattedPostfix);
			}
			description.Add("CompPre", stringBuilder.ToString());
			description.Add("CompPost", stringBuilder2.ToString());
		}

		public virtual string PostProcessDescription(string description, PileType pileType, DescriptionPreviewType previewType, Creature? target = null)
		{
			if (!string.IsNullOrEmpty(description))
			{
				return string.Join('\n', description.Split('\n', StringSplitOptions.RemoveEmptyEntries));
			}
			return description;
		}

		public ICardComponent? AddComponent<T>(T incoming, bool allowMerge = true, bool isUpgrade = false) where T : class, ICardComponent
		{
			return ApplyComponent(incoming, new ApplyComponentOptions(allowMerge, UseSubtractiveMerge: false, isUpgrade));
		}

		public ICardComponent? SubtractComponent<T>(T incoming, bool isUpgrade = false) where T : class, ICardComponent
		{
			return ApplyComponent(incoming, new ApplyComponentOptions(AllowMerge: true, UseSubtractiveMerge: true, isUpgrade));
		}

		public ICardComponent? ApplyComponent<T>(T incoming, ApplyComponentOptions options = default(ApplyComponentOptions)) where T : class, ICardComponent
		{
			EnsureComponentsInitialized();
			if (options.AllowMerge)
			{
				for (int i = 0; i < _components.Count; i++)
				{
					ICardComponent cardComponent = _components[i];
					if (options.UseSubtractiveMerge ? cardComponent.TrySubtractiveMergeWith(incoming, options, out ICardComponent merged) : cardComponent.TryMergeWith(incoming, options, out merged))
					{
						if (merged == cardComponent)
						{
							return cardComponent;
						}
						cardComponent.Detach();
						if (merged == null)
						{
							_components.RemoveAt(i);
							return null;
						}
						_components[i] = merged;
						merged.Attach(this);
						return merged;
					}
				}
			}
			if (options.UseSubtractiveMerge)
			{
				return null;
			}
			_components.Add(incoming);
			incoming.Attach(this);
			return incoming;
		}

		public ICardComponent? RemoveComponent<T>() where T : class, ICardComponent
		{
			EnsureComponentsInitialized();
			int num = _components.FindIndex((ICardComponent c) => c is T);
			if (num < 0)
			{
				return null;
			}
			ICardComponent result = _components[num];
			_components[num].Detach();
			_components.RemoveAt(num);
			return result;
		}

		public IReadOnlyList<ICardComponent> RemoveComponents<T>() where T : class, ICardComponent
		{
			EnsureComponentsInitialized();
			List<ICardComponent> list = new List<ICardComponent>();
			for (int num = _components.Count - 1; num >= 0; num--)
			{
				if (_components[num] is T val)
				{
					val.Detach();
					_components.RemoveAt(num);
					list.Add(val);
				}
			}
			list.Reverse();
			return list;
		}

		public bool RefRemoveComponent(ICardComponent component)
		{
			EnsureComponentsInitialized();
			int num = _components.FindIndex((ICardComponent c) => c == component);
			if (num < 0)
			{
				return false;
			}
			_components[num].Detach();
			_components.RemoveAt(num);
			return true;
		}

		public T? GetComponent<T>() where T : class, ICardComponent
		{
			EnsureComponentsInitialized();
			return _components.OfType<T>().FirstOrDefault();
		}

		public IReadOnlyList<T> GetComponents<T>() where T : class, ICardComponent
		{
			EnsureComponentsInitialized();
			return _components.OfType<T>().ToArray();
		}

		public void EnsureComponentsInitialized()
		{
			if (_components != null)
			{
				return;
			}
			if (_componentStateBlob.Length == 0)
			{
				_components = new List<ICardComponent>();
				foreach (ICardComponent canonicalComponent in CanonicalComponents)
				{
					ICardComponent cardComponent = canonicalComponent.DeepClone();
					_components.Add(cardComponent);
					cardComponent.Attach(this);
				}
			}
			else
			{
				_components = CardComponentStateSerializer.Deserialize(_componentStateBlob, this);
			}
			_componentStateBlob = CardComponentStateSerializer.Serialize(_components);
		}

		public bool CanHandleRightClickLocal(RightClickContext context)
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.CanHandleRightClickLocal(context)))
			{
				return CanHandleRightClickLocalC(context);
			}
			return true;
		}

		public async Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			ICardComponent[] array = _components.ToArray();
			foreach (ICardComponent cardComponent in array)
			{
				if (cardComponent.CanHandleRightClick(clickContext))
				{
					flag = true;
					await cardComponent.OnRightClick(choiceContext, clickContext);
					break;
				}
			}
			if (!flag)
			{
				await OnRightClickC(choiceContext, clickContext);
			}
		}

		protected virtual Dictionary<string, object> GenerateCommonExtraArgsForComponents(PileType pileType, DescriptionPreviewType previewType, Creature? target = null)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Expected O, but got Unknown
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Invalid comparison between Unknown and I4
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Invalid comparison between Unknown and I4
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			UpgradeDisplay val = (UpgradeDisplay)((previewType == DescriptionPreviewType.Upgrade) ? 2 : (((CardModel)this).IsUpgraded ? 1 : 0));
			dictionary["IfUpgraded"] = (object)new IfUpgradedVar(val);
			bool flag = (((int)pileType == 2 || (int)pileType == 5) ? true : false);
			bool flag2 = flag;
			dictionary["OnTable"] = flag2;
			int num;
			if (CombatManager.Instance.IsInProgress)
			{
				CardPile pile = ((CardModel)this).Pile;
				num = (((pile != null) ? pile.IsCombatPile : PileTypeExtensions.IsCombatPile(pileType)) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
			bool flag3 = (byte)num != 0;
			dictionary["InCombat"] = flag3;
			dictionary["IsTargeting"] = target != null;
			dictionary["TargetType"] = ((object)((CardModel)this).TargetType/*cast due to constrained. prefix*/).ToString();
			string prefix = EnergyIconHelper.GetPrefix((AbstractModel)(object)this);
			dictionary["energyPrefix"] = prefix;
			dictionary["singleStarIcon"] = "[img]res://images/packed/sprite_fonts/star_icon.png[/img]";
			return dictionary;
		}

		protected override void DeepCloneFields()
		{
			((CardModel)this).DeepCloneFields();
			if (_components == null)
			{
				return;
			}
			_components = _components.Select((ICardComponent c) => c.DeepClone()).ToList();
			foreach (ICardComponent component in _components)
			{
				component.Attach(this, isInternal: true);
			}
			_componentStateBlob = CardComponentStateSerializer.Serialize(_components);
		}

		protected override void AfterDeserialized()
		{
			((CardModel)this).AfterDeserialized();
			_components = null;
			EnsureComponentsInitialized();
		}

		protected sealed override PileType GetResultPileTypeForCardPlay()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			foreach (ICardComponent component in _components)
			{
				PileType? resultPileTypeForCardPlay = component.GetResultPileTypeForCardPlay();
				if (resultPileTypeForCardPlay.HasValue)
				{
					return resultPileTypeForCardPlay.GetValueOrDefault();
				}
			}
			return GetResultPileTypeForCardPlayC();
		}

		protected virtual PileType GetResultPileTypeForCardPlayC()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ((CardModel)this).GetResultPileTypeForCardPlay();
		}

		private void HandlePhaseTransitionLimitExceeded(ComponentPhase lastPhase)
		{
			Log.Warn($"Card '{((AbstractModel)this).Id.Entry}' exceeded the maximum of {MaxPhaseTransitions} phase transitions. Last phase: {lastPhase}.\n       This likely indicates an infinite loop in the card's logic, and no further phase transitions will be processed to prevent game instability.\n       At the time, there are {_components.Count} component(s) attached to the card, with the following types:\n       {string.Join(", ", _components.Select((ICardComponent c) => c.ComponentId))}\n       If you are sure it's a false positive, try modify ComponentsCardModel.MaxPhaseTransitions via reflection.", 2);
		}

		protected virtual bool CanHandleRightClickLocalC(RightClickContext context)
		{
			return false;
		}

		protected virtual Task OnRightClickC(PlayerChoiceContext choiceContext, RightClickContext clickContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		protected sealed override void OnUpgrade()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Core);
			ICardComponent[] array = _components.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].OnUpgrade(componentContext);
			}
			OnUpgrade(componentContext);
		}

		protected virtual void OnUpgrade(ComponentContext componentContext)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		protected sealed override void AfterDowngraded()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Core);
			ICardComponent[] array = _components.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				array[i].AfterDowngraded(componentContext);
			}
			AfterDowngraded(componentContext);
		}

		protected virtual void AfterDowngraded(ComponentContext componentContext)
		{
		}

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		public virtual Task ComponentCallBack(string name, params object?[] args)
		{
			return Task.CompletedTask;
		}

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		public virtual bool ComponentPredicate(string name, params object?[] args)
		{
			return false;
		}

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		public virtual object? ComponentQuery(string name, params object?[] args)
		{
			return null;
		}

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		public virtual Task<object?> ComponentQueryAsync(string name, params object?[] args)
		{
			return Task.FromResult<object>(null);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		protected sealed override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.OnPlayPrefix(choiceContext, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.OnPlayPostfix(choiceContext, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await OnPlayPhased(choiceContext, cardPlay, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task OnPlayPhased(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return OnPlay(choiceContext, cardPlay, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task OnEnqueuePlayVfx(Creature? target)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.OnEnqueuePlayVfxPrefix(target, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.OnEnqueuePlayVfxPostfix(target, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await OnEnqueuePlayVfxPhased(target, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task OnEnqueuePlayVfxPhased(Creature? target, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return OnEnqueuePlayVfx(target, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task OnEnqueuePlayVfx(Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		protected sealed override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.OnTurnEndInHandPrefix(choiceContext, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.OnTurnEndInHandPostfix(choiceContext, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await OnTurnEndInHandPhased(choiceContext, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task OnTurnEndInHandPhased(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return OnTurnEndInHand(choiceContext, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task OnTurnEndInHand(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeCardPlayed(CardPlay cardPlay)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeCardPlayedPrefix(cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeCardPlayedPostfix(cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeCardPlayedPhased(cardPlay, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeCardPlayedPhased(CardPlay cardPlay, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeCardPlayed(cardPlay, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeCardPlayed(CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardPlayedPrefix(choiceContext, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardPlayedPostfix(choiceContext, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardPlayedPhased(choiceContext, cardPlay, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardPlayedPhased(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardPlayed(choiceContext, cardPlay, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardPlayedLatePrefix(choiceContext, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardPlayedLatePostfix(choiceContext, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardPlayedLatePhased(choiceContext, cardPlay, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardPlayedLatePhased(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardPlayedLate(choiceContext, cardPlay, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardPlayedLate(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPlayerTurnStartEarlyPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPlayerTurnStartEarlyPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPlayerTurnStartEarlyPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPlayerTurnStartEarlyPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPlayerTurnStartEarly(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPlayerTurnStartEarly(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPlayerTurnStartPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPlayerTurnStartPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPlayerTurnStartPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPlayerTurnStartPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPlayerTurnStart(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPlayerTurnStartLatePrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPlayerTurnStartLatePostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPlayerTurnStartLatePhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPlayerTurnStartLatePhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPlayerTurnStartLate(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPlayerTurnStartLate(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterAutoPostPlayPhaseEnteredPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterAutoPostPlayPhaseEnteredPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterAutoPostPlayPhaseEnteredPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterAutoPostPlayPhaseEnteredPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterAutoPostPlayPhaseEntered(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterAutoPrePlayPhaseEnteredEarlyPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterAutoPrePlayPhaseEnteredEarlyPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterAutoPrePlayPhaseEnteredEarlyPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterAutoPrePlayPhaseEnteredEarlyPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterAutoPrePlayPhaseEnteredEarly(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterAutoPrePlayPhaseEnteredPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterAutoPrePlayPhaseEnteredPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterAutoPrePlayPhaseEnteredPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterAutoPrePlayPhaseEnteredPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterAutoPrePlayPhaseEntered(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterAutoPrePlayPhaseEnteredLatePrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterAutoPrePlayPhaseEnteredLatePostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterAutoPrePlayPhaseEnteredLatePhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterAutoPrePlayPhaseEnteredLatePhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterAutoPrePlayPhaseEnteredLate(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterAutoPrePlayPhaseEnteredLate(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeSideTurnEndVeryEarlyPrefix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeSideTurnEndVeryEarlyPostfix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeSideTurnEndVeryEarlyPhased(choiceContext, side, participants, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeSideTurnEndVeryEarlyPhased(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeSideTurnEndVeryEarly(choiceContext, side, participants, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeSideTurnEndVeryEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeSideTurnEndEarlyPrefix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeSideTurnEndEarlyPostfix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeSideTurnEndEarlyPhased(choiceContext, side, participants, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeSideTurnEndEarlyPhased(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeSideTurnEndEarly(choiceContext, side, participants, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeSideTurnEndEarly(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeSideTurnEndPrefix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeSideTurnEndPostfix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeSideTurnEndPhased(choiceContext, side, participants, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeSideTurnEndPhased(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeSideTurnEnd(choiceContext, side, participants, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterSideTurnEndPrefix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterSideTurnEndPostfix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterSideTurnEndPhased(choiceContext, side, participants, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterSideTurnEndPhased(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterSideTurnEnd(choiceContext, side, participants, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterSideTurnEndLatePrefix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterSideTurnEndLatePostfix(choiceContext, side, participants, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterSideTurnEndLatePhased(choiceContext, side, participants, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterSideTurnEndLatePhased(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterSideTurnEndLate(choiceContext, side, participants, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterSideTurnEndLate(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterActEntered()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterActEnteredPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterActEnteredPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterActEnteredPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterActEnteredPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterActEntered(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterActEntered(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterAddToDeckPrevented(CardModel card)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterAddToDeckPreventedPrefix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterAddToDeckPreventedPostfix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterAddToDeckPreventedPhased(card, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterAddToDeckPreventedPhased(CardModel card, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterAddToDeckPrevented(card, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterAddToDeckPrevented(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeAttack(AttackCommand command)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeAttackPrefix(command, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeAttackPostfix(command, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeAttackPhased(command, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeAttackPhased(AttackCommand command, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeAttack(command, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeAttack(AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterAttackPrefix(choiceContext, command, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterAttackPostfix(choiceContext, command, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterAttackPhased(choiceContext, command, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterAttackPhased(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterAttack(choiceContext, command, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterBlockCleared(Creature creature)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterBlockClearedPrefix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterBlockClearedPostfix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterBlockClearedPhased(creature, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterBlockClearedPhased(Creature creature, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterBlockCleared(creature, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterBlockCleared(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeBlockGainedPrefix(creature, amount, props, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeBlockGainedPostfix(creature, amount, props, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeBlockGainedPhased(creature, amount, props, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeBlockGainedPhased(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeBlockGained(creature, amount, props, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterBlockGainedPrefix(creature, amount, props, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterBlockGainedPostfix(creature, amount, props, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterBlockGainedPhased(creature, amount, props, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterBlockGainedPhased(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterBlockGained(creature, amount, props, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterBlockGained(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterBlockBroken(Creature creature)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterBlockBrokenPrefix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterBlockBrokenPostfix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterBlockBrokenPhased(creature, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterBlockBrokenPhased(Creature creature, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterBlockBroken(creature, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterBlockBroken(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardChangedPilesPrefix(card, oldPileType, source, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardChangedPilesPostfix(card, oldPileType, source, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardChangedPilesPhased(card, oldPileType, source, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardChangedPilesPhased(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardChangedPiles(card, oldPileType, source, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? source)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardChangedPilesLatePrefix(card, oldPileType, source, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardChangedPilesLatePostfix(card, oldPileType, source, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardChangedPilesLatePhased(card, oldPileType, source, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardChangedPilesLatePhased(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardChangedPilesLate(card, oldPileType, source, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardChangedPilesLate(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardDiscardedPrefix(choiceContext, card, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardDiscardedPostfix(choiceContext, card, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardDiscardedPhased(choiceContext, card, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardDiscardedPhased(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardDiscarded(choiceContext, card, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardDrawnEarlyPrefix(choiceContext, card, fromHandDraw, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardDrawnEarlyPostfix(choiceContext, card, fromHandDraw, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardDrawnEarlyPhased(choiceContext, card, fromHandDraw, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardDrawnEarlyPhased(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardDrawnEarly(choiceContext, card, fromHandDraw, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardDrawnEarly(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardDrawnPrefix(choiceContext, card, fromHandDraw, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardDrawnPostfix(choiceContext, card, fromHandDraw, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardDrawnPhased(choiceContext, card, fromHandDraw, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardDrawnPhased(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardDrawn(choiceContext, card, fromHandDraw, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardEnteredCombat(CardModel card)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardEnteredCombatPrefix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardEnteredCombatPostfix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardEnteredCombatPhased(card, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardEnteredCombatPhased(CardModel card, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardEnteredCombat(card, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardEnteredCombat(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardGeneratedForCombatPrefix(card, creator, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardGeneratedForCombatPostfix(card, creator, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardGeneratedForCombatPhased(card, creator, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardGeneratedForCombatPhased(CardModel card, Player? creator, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardGeneratedForCombat(card, creator, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardGeneratedForCombat(CardModel card, Player? creator, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCardExhaustedPrefix(choiceContext, card, causedByEthereal, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCardExhaustedPostfix(choiceContext, card, causedByEthereal, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCardExhaustedPhased(choiceContext, card, causedByEthereal, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCardExhaustedPhased(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCardExhausted(choiceContext, card, causedByEthereal, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeCardAutoPlayed(CardModel card, Creature? target, AutoPlayType type)
		{
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeCardAutoPlayedPrefix(card, target, type, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeCardAutoPlayedPostfix(card, target, type, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeCardAutoPlayedPhased(card, target, type, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeCardAutoPlayedPhased(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeCardAutoPlayed(card, target, type, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeCardAutoPlayed(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeCombatStart()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeCombatStartPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeCombatStartPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeCombatStartPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeCombatStartPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeCombatStart(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeCombatStart(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeCombatStartLate()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeCombatStartLatePrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeCombatStartLatePostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeCombatStartLatePhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeCombatStartLatePhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeCombatStartLate(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeCombatStartLate(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCombatEnd(CombatRoom room)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCombatEndPrefix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCombatEndPostfix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCombatEndPhased(room, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCombatEndPhased(CombatRoom room, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCombatEnd(room, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCombatEnd(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCombatVictoryEarly(CombatRoom room)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCombatVictoryEarlyPrefix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCombatVictoryEarlyPostfix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCombatVictoryEarlyPhased(room, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCombatVictoryEarlyPhased(CombatRoom room, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCombatVictoryEarly(room, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCombatVictoryEarly(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCombatVictory(CombatRoom room)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCombatVictoryPrefix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCombatVictoryPostfix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCombatVictoryPhased(room, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCombatVictoryPhased(CombatRoom room, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCombatVictory(room, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCombatVictory(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCreatureAddedToCombat(Creature creature)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCreatureAddedToCombatPrefix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCreatureAddedToCombatPostfix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCreatureAddedToCombatPhased(creature, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCreatureAddedToCombatPhased(Creature creature, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCreatureAddedToCombat(creature, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCreatureAddedToCombat(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterCurrentHpChangedPrefix(creature, delta, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterCurrentHpChangedPostfix(creature, delta, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterCurrentHpChangedPhased(creature, delta, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterCurrentHpChangedPhased(Creature creature, decimal delta, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterCurrentHpChanged(creature, delta, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterCurrentHpChanged(Creature creature, decimal delta, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterDamageGivenPrefix(choiceContext, dealer, result, props, target, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterDamageGivenPostfix(choiceContext, dealer, result, props, target, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterDamageGivenPhased(choiceContext, dealer, result, props, target, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterDamageGivenPhased(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterDamageGiven(choiceContext, dealer, result, props, target, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeDamageReceivedPrefix(choiceContext, target, amount, props, dealer, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeDamageReceivedPostfix(choiceContext, target, amount, props, dealer, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeDamageReceivedPhased(choiceContext, target, amount, props, dealer, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeDamageReceivedPhased(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeDamageReceived(choiceContext, target, amount, props, dealer, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterDamageReceivedPrefix(choiceContext, target, result, props, dealer, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterDamageReceivedPostfix(choiceContext, target, result, props, dealer, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterDamageReceivedPhased(choiceContext, target, result, props, dealer, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterDamageReceivedPhased(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterDamageReceived(choiceContext, target, result, props, dealer, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterDamageReceivedLatePrefix(choiceContext, target, result, props, dealer, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterDamageReceivedLatePostfix(choiceContext, target, result, props, dealer, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterDamageReceivedLatePhased(choiceContext, target, result, props, dealer, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterDamageReceivedLatePhased(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterDamageReceivedLate(choiceContext, target, result, props, dealer, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterDamageReceivedLate(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeDeath(Creature creature)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeDeathPrefix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeDeathPostfix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeDeathPhased(creature, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeDeathPhased(Creature creature, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeDeath(creature, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeDeath(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterDeathPrefix(choiceContext, creature, wasRemovalPrevented, deathAnimLength, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterDeathPostfix(choiceContext, creature, wasRemovalPrevented, deathAnimLength, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterDeathPhased(choiceContext, creature, wasRemovalPrevented, deathAnimLength, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterDeathPhased(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterDeath(choiceContext, creature, wasRemovalPrevented, deathAnimLength, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterDiedToDoomPrefix(choiceContext, creatures, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterDiedToDoomPostfix(choiceContext, creatures, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterDiedToDoomPhased(choiceContext, creatures, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterDiedToDoomPhased(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterDiedToDoom(choiceContext, creatures, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterDiedToDoom(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterEnergyReset(Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterEnergyResetPrefix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterEnergyResetPostfix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterEnergyResetPhased(player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterEnergyResetPhased(Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterEnergyReset(player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterEnergyReset(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterEnergyResetLate(Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterEnergyResetLatePrefix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterEnergyResetLatePostfix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterEnergyResetLatePhased(player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterEnergyResetLatePhased(Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterEnergyResetLate(player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterEnergyResetLate(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterEnergySpent(CardModel card, int amount)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterEnergySpentPrefix(card, amount, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterEnergySpentPostfix(card, amount, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterEnergySpentPhased(card, amount, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterEnergySpentPhased(CardModel card, int amount, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterEnergySpent(card, amount, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterEnergySpent(CardModel card, int amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeCardRemoved(CardModel card)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeCardRemovedPrefix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeCardRemovedPostfix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeCardRemovedPhased(card, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeCardRemovedPhased(CardModel card, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeCardRemoved(card, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeCardRemoved(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeFlushPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeFlushPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeFlushPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeFlushPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeFlush(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeFlush(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeFlushLatePrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeFlushLatePostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeFlushLatePhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeFlushLatePhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeFlushLate(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeFlushLate(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterFlushPrefix(choiceContext, player, flushedCards, retainedCards, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterFlushPostfix(choiceContext, player, flushedCards, retainedCards, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterFlushPhased(choiceContext, player, flushedCards, retainedCards, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterFlushPhased(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterFlush(choiceContext, player, flushedCards, retainedCards, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterFlush(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterGoldGained(Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterGoldGainedPrefix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterGoldGainedPostfix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterGoldGainedPhased(player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterGoldGainedPhased(Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterGoldGained(player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterGoldGained(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeHandDrawPrefix(player, choiceContext, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeHandDrawPostfix(player, choiceContext, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeHandDrawPhased(player, choiceContext, combatState, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeHandDrawPhased(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeHandDraw(player, choiceContext, combatState, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeHandDrawLatePrefix(player, choiceContext, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeHandDrawLatePostfix(player, choiceContext, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeHandDrawLatePhased(player, choiceContext, combatState, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeHandDrawLatePhased(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeHandDrawLate(player, choiceContext, combatState, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeHandDrawLate(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterHandEmptiedPrefix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterHandEmptiedPostfix(choiceContext, player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterHandEmptiedPhased(choiceContext, player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterHandEmptiedPhased(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterHandEmptied(choiceContext, player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterHandEmptied(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterItemPurchasedPrefix(player, itemPurchased, goldSpent, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterItemPurchasedPostfix(player, itemPurchased, goldSpent, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterItemPurchasedPhased(player, itemPurchased, goldSpent, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterItemPurchasedPhased(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterItemPurchased(player, itemPurchased, goldSpent, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterItemPurchased(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterMapGenerated(ActMap map, int actIndex)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterMapGeneratedPrefix(map, actIndex, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterMapGeneratedPostfix(map, actIndex, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterMapGeneratedPhased(map, actIndex, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterMapGeneratedPhased(ActMap map, int actIndex, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterMapGenerated(map, actIndex, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterMapGenerated(ActMap map, int actIndex, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingBlockAmountPrefix(modifiedAmount, cardSource, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingBlockAmountPostfix(modifiedAmount, cardSource, cardPlay, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingBlockAmountPhased(modifiedAmount, cardSource, cardPlay, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingBlockAmountPhased(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingBlockAmount(modifiedAmount, cardSource, cardPlay, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingCardPlayCount(CardModel card)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingCardPlayCountPrefix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingCardPlayCountPostfix(card, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingCardPlayCountPhased(card, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingCardPlayCountPhased(CardModel card, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingCardPlayCount(card, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingCardPlayCount(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingCardPlayResultPileOrPosition(CardModel card, PileType pileType, CardPilePosition position)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingCardPlayResultPileOrPositionPrefix(card, pileType, position, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingCardPlayResultPileOrPositionPostfix(card, pileType, position, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingCardPlayResultPileOrPositionPhased(card, pileType, position, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingCardPlayResultPileOrPositionPhased(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingCardPlayResultPileOrPosition(card, pileType, position, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingCardPlayResultPileOrPosition(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingOrbPassiveTriggerCount(OrbModel orb)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingOrbPassiveTriggerCountPrefix(orb, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingOrbPassiveTriggerCountPostfix(orb, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingOrbPassiveTriggerCountPhased(orb, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingOrbPassiveTriggerCountPhased(OrbModel orb, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingOrbPassiveTriggerCount(orb, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingOrbPassiveTriggerCount(OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingCardRewardOptions()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingCardRewardOptionsPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingCardRewardOptionsPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingCardRewardOptionsPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingCardRewardOptionsPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingCardRewardOptions(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingCardRewardOptions(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingDamageAmount(CardModel? cardSource)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingDamageAmountPrefix(cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingDamageAmountPostfix(cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingDamageAmountPhased(cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingDamageAmountPhased(CardModel? cardSource, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingDamageAmount(cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingDamageAmount(CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingEnergyGain()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingEnergyGainPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingEnergyGainPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingEnergyGainPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingEnergyGainPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingEnergyGain(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingEnergyGain(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingHandDraw()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingHandDrawPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingHandDrawPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingHandDrawPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingHandDrawPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingHandDraw(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingHandDraw(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPreventingDraw()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPreventingDrawPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPreventingDrawPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPreventingDrawPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPreventingDrawPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPreventingDraw(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPreventingDraw(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingHpLostBeforeOsty()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingHpLostBeforeOstyPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingHpLostBeforeOstyPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingHpLostBeforeOstyPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingHpLostBeforeOstyPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingHpLostBeforeOsty(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingHpLostBeforeOsty(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingHpLostAfterOsty()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingHpLostAfterOstyPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingHpLostAfterOstyPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingHpLostAfterOstyPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingHpLostAfterOstyPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingHpLostAfterOsty(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingHpLostAfterOsty(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingPowerAmountReceived(PowerModel power)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingPowerAmountReceivedPrefix(power, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingPowerAmountReceivedPostfix(power, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingPowerAmountReceivedPhased(power, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingPowerAmountReceivedPhased(PowerModel power, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingPowerAmountReceived(power, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingPowerAmountReceived(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingPowerAmountGiven(PowerModel power)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingPowerAmountGivenPrefix(power, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingPowerAmountGivenPostfix(power, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingPowerAmountGivenPhased(power, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingPowerAmountGivenPhased(PowerModel power, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingPowerAmountGiven(power, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingPowerAmountGiven(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingRewards()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingRewardsPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingRewardsPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingRewardsPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingRewardsPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingRewards(componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingRewards(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterOrbChanneledPrefix(choiceContext, player, orb, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterOrbChanneledPostfix(choiceContext, player, orb, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterOrbChanneledPhased(choiceContext, player, orb, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterOrbChanneledPhased(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterOrbChanneled(choiceContext, player, orb, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterOrbChanneled(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterOrbEvokedPrefix(choiceContext, orb, targets, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterOrbEvokedPostfix(choiceContext, orb, targets, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterOrbEvokedPhased(choiceContext, orb, targets, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterOrbEvokedPhased(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterOrbEvoked(choiceContext, orb, targets, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterOstyRevived(Creature osty)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterOstyRevivedPrefix(osty, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterOstyRevivedPostfix(osty, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterOstyRevivedPhased(osty, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterOstyRevivedPhased(Creature osty, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterOstyRevived(osty, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterOstyRevived(Creature osty, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforePotionUsed(PotionModel potion, Creature? target)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforePotionUsedPrefix(potion, target, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforePotionUsedPostfix(potion, target, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforePotionUsedPhased(potion, target, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforePotionUsedPhased(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforePotionUsed(potion, target, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforePotionUsed(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPotionUsed(PotionModel potion, Creature? target)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPotionUsedPrefix(potion, target, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPotionUsedPostfix(potion, target, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPotionUsedPhased(potion, target, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPotionUsedPhased(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPotionUsed(potion, target, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPotionUsed(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPotionDiscarded(PotionModel potion)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPotionDiscardedPrefix(potion, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPotionDiscardedPostfix(potion, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPotionDiscardedPhased(potion, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPotionDiscardedPhased(PotionModel potion, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPotionDiscarded(potion, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPotionDiscarded(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPotionProcured(PotionModel potion)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPotionProcuredPrefix(potion, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPotionProcuredPostfix(potion, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPotionProcuredPhased(potion, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPotionProcuredPhased(PotionModel potion, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPotionProcured(potion, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPotionProcured(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforePowerAmountChangedPrefix(power, amount, target, applier, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforePowerAmountChangedPostfix(power, amount, target, applier, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforePowerAmountChangedPhased(power, amount, target, applier, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforePowerAmountChangedPhased(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforePowerAmountChanged(power, amount, target, applier, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforePowerAmountChanged(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPowerAmountChangedPrefix(choiceContext, power, amount, applier, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPowerAmountChangedPostfix(choiceContext, power, amount, applier, cardSource, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPowerAmountChangedPhased(choiceContext, power, amount, applier, cardSource, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPowerAmountChangedPhased(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPowerAmountChanged(choiceContext, power, amount, applier, cardSource, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPreventingBlockClearPrefix(preventer, creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPreventingBlockClearPostfix(preventer, creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPreventingBlockClearPhased(preventer, creature, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPreventingBlockClearPhased(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPreventingBlockClear(preventer, creature, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPreventingBlockClear(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterPreventingDeath(Creature creature)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterPreventingDeathPrefix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterPreventingDeathPostfix(creature, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterPreventingDeathPhased(creature, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterPreventingDeathPhased(Creature creature, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterPreventingDeath(creature, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterPreventingDeath(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterRestSiteHeal(Player player, bool isMimicked)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterRestSiteHealPrefix(player, isMimicked, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterRestSiteHealPostfix(player, isMimicked, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterRestSiteHealPhased(player, isMimicked, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterRestSiteHealPhased(Player player, bool isMimicked, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterRestSiteHeal(player, isMimicked, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterRestSiteHeal(Player player, bool isMimicked, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterRestSiteSmith(Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterRestSiteSmithPrefix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterRestSiteSmithPostfix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterRestSiteSmithPhased(player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterRestSiteSmithPhased(Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterRestSiteSmith(player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterRestSiteSmith(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterRewardTaken(Player player, Reward reward)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterRewardTakenPrefix(player, reward, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterRewardTakenPostfix(player, reward, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterRewardTakenPhased(player, reward, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterRewardTakenPhased(Player player, Reward reward, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterRewardTaken(player, reward, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterRewardTaken(Player player, Reward reward, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeRoomEntered(AbstractRoom room)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeRoomEnteredPrefix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeRoomEnteredPostfix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeRoomEnteredPhased(room, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeRoomEnteredPhased(AbstractRoom room, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeRoomEntered(room, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeRoomEntered(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterRoomEntered(AbstractRoom room)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterRoomEnteredPrefix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterRoomEnteredPostfix(room, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterRoomEnteredPhased(room, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterRoomEnteredPhased(AbstractRoom room, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterRoomEntered(room, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterRoomEntered(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterShufflePrefix(choiceContext, shuffler, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterShufflePostfix(choiceContext, shuffler, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterShufflePhased(choiceContext, shuffler, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterShufflePhased(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterShuffle(choiceContext, shuffler, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterShuffle(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterStarsSpent(int amount, Player spender)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterStarsSpentPrefix(amount, spender, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterStarsSpentPostfix(amount, spender, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterStarsSpentPhased(amount, spender, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterStarsSpentPhased(int amount, Player spender, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterStarsSpent(amount, spender, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterStarsSpent(int amount, Player spender, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterStarsGained(int amount, Player gainer)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterStarsGainedPrefix(amount, gainer, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterStarsGainedPostfix(amount, gainer, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterStarsGainedPhased(amount, gainer, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterStarsGainedPhased(int amount, Player gainer, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterStarsGained(amount, gainer, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterStarsGained(int amount, Player gainer, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterForge(decimal amount, Player forger, AbstractModel? source)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterForgePrefix(amount, forger, source, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterForgePostfix(amount, forger, source, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterForgePhased(amount, forger, source, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterForgePhased(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterForge(amount, forger, source, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterForge(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterSummon(PlayerChoiceContext choiceContext, Player summoner, decimal amount)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterSummonPrefix(choiceContext, summoner, amount, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterSummonPostfix(choiceContext, summoner, amount, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterSummonPhased(choiceContext, summoner, amount, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterSummonPhased(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterSummon(choiceContext, summoner, amount, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterSummon(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterTakingExtraTurn(Player player)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterTakingExtraTurnPrefix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterTakingExtraTurnPostfix(player, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterTakingExtraTurnPhased(player, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterTakingExtraTurnPhased(Player player, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterTakingExtraTurn(player, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterTakingExtraTurn(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterTargetingBlockedVfx(Creature blocker)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterTargetingBlockedVfxPrefix(blocker, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterTargetingBlockedVfxPostfix(blocker, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterTargetingBlockedVfxPhased(blocker, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterTargetingBlockedVfxPhased(Creature blocker, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterTargetingBlockedVfx(blocker, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterTargetingBlockedVfx(Creature blocker, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.BeforeSideTurnStartPrefix(choiceContext, side, participants, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.BeforeSideTurnStartPostfix(choiceContext, side, participants, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await BeforeSideTurnStartPhased(choiceContext, side, participants, combatState, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task BeforeSideTurnStartPhased(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return BeforeSideTurnStart(choiceContext, side, participants, combatState, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterSideTurnStartPrefix(side, participants, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterSideTurnStartPostfix(side, participants, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterSideTurnStartPhased(side, participants, combatState, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterSideTurnStartPhased(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterSideTurnStart(side, participants, combatState, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterSideTurnStartLatePrefix(side, participants, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterSideTurnStartLatePostfix(side, participants, combatState, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterSideTurnStartLatePhased(side, participants, combatState, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterSideTurnStartLatePhased(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterSideTurnStartLate(side, participants, combatState, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override async Task AfterModifyingGoldGained(Player player, decimal amount)
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] snapshot = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				snapshot[i] = Components[i];
			}
			try
			{
				for (int transitions = 0; transitions < MaxPhaseTransitions; transitions++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int i2 = 0; i2 < count; i2++)
						{
							ICardComponent cardComponent2 = snapshot[i2];
							if (cardComponent2.ComponentsCard == this)
							{
								await cardComponent2.AfterModifyingGoldGainedPrefix(player, amount, componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int i2 = count - 1; i2 >= 0; i2--)
						{
							ICardComponent cardComponent = snapshot[i2];
							if (cardComponent.ComponentsCard == this)
							{
								await cardComponent.AfterModifyingGoldGainedPostfix(player, amount, componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						await AfterModifyingGoldGainedPhased(player, amount, componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(snapshot, clearArray: true);
			}
		}

		protected virtual Task AfterModifyingGoldGainedPhased(Player player, decimal amount, ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				return AfterModifyingGoldGained(player, amount, componentContext);
			}
			return Task.CompletedTask;
		}

		protected virtual Task AfterModifyingGoldGained(Player player, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override void AfterTransformedFrom()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] array = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				array[i] = Components[i];
			}
			try
			{
				for (int j = 0; j < MaxPhaseTransitions; j++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int k = 0; k < count; k++)
						{
							ICardComponent cardComponent2 = array[k];
							if (cardComponent2.ComponentsCard == this)
							{
								cardComponent2.AfterTransformedFromPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int num = count - 1; num >= 0; num--)
						{
							ICardComponent cardComponent = array[num];
							if (cardComponent.ComponentsCard == this)
							{
								cardComponent.AfterTransformedFromPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						AfterTransformedFromPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(array, clearArray: true);
			}
		}

		protected virtual void AfterTransformedFromPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				AfterTransformedFrom(componentContext);
			}
		}

		protected virtual void AfterTransformedFrom(ComponentContext componentContext)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try adding `ComponentContext componentContext` as the last parameter, or disable this warning if intended.", false)]
		public sealed override void AfterTransformedTo()
		{
			EnsureComponentsInitialized();
			ComponentContext componentContext = new ComponentContext(ComponentPhase.Init);
			int count = Components.Count;
			ICardComponent[] array = ArrayPool<ICardComponent>.Shared.Rent(count);
			for (int i = 0; i < count; i++)
			{
				array[i] = Components[i];
			}
			try
			{
				for (int j = 0; j < MaxPhaseTransitions; j++)
				{
					if (componentContext.Phase == ComponentPhase.Final)
					{
						break;
					}
					componentContext.MoveNextPhase();
					switch (componentContext.Phase)
					{
					case ComponentPhase.Prefix:
					{
						for (int k = 0; k < count; k++)
						{
							ICardComponent cardComponent2 = array[k];
							if (cardComponent2.ComponentsCard == this)
							{
								cardComponent2.AfterTransformedToPrefix(componentContext);
								if (componentContext.Phase != ComponentPhase.Prefix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Postfix:
					{
						for (int num = count - 1; num >= 0; num--)
						{
							ICardComponent cardComponent = array[num];
							if (cardComponent.ComponentsCard == this)
							{
								cardComponent.AfterTransformedToPostfix(componentContext);
								if (componentContext.Phase != ComponentPhase.Postfix)
								{
									break;
								}
							}
						}
						break;
					}
					case ComponentPhase.Prime:
					case ComponentPhase.Core:
					case ComponentPhase.Final:
						AfterTransformedToPhased(componentContext);
						break;
					default:
						throw new ArgumentOutOfRangeException();
					}
				}
				if (componentContext.Phase != ComponentPhase.Final)
				{
					HandlePhaseTransitionLimitExceeded(componentContext.Phase);
				}
			}
			finally
			{
				ArrayPool<ICardComponent>.Shared.Return(array, clearArray: true);
			}
		}

		protected virtual void AfterTransformedToPhased(ComponentContext componentContext)
		{
			if (componentContext.Phase == ComponentPhase.Core)
			{
				AfterTransformedTo(componentContext);
			}
		}

		protected virtual void AfterTransformedTo(ComponentContext componentContext)
		{
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override int ModifyAttackHitCount(AttackCommand attack, int hitCount)
		{
			EnsureComponentsInitialized();
			int hitCount2 = hitCount;
			foreach (ICardComponent component in _components)
			{
				hitCount2 = component.ModifyAttackHitCount(attack, hitCount2);
			}
			return ModifyAttackHitCountC(attack, hitCount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal block2 = block;
			foreach (ICardComponent component in _components)
			{
				block2 = component.ModifyBlockAdditive(target, block2, props, cardSource, cardPlay);
			}
			return ModifyBlockAdditiveC(target, block2, props, cardSource, cardPlay);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal block2 = block;
			foreach (ICardComponent component in _components)
			{
				block2 = component.ModifyBlockMultiplicative(target, block2, props, cardSource, cardPlay);
			}
			return ModifyBlockMultiplicativeC(target, block2, props, cardSource, cardPlay);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			EnsureComponentsInitialized();
			int playCount2 = playCount;
			foreach (ICardComponent component in _components)
			{
				playCount2 = component.ModifyCardPlayCount(card, target, playCount2);
			}
			return ModifyCardPlayCountC(card, target, playCount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			(PileType, CardPilePosition) tuple = (pileType, position);
			foreach (ICardComponent component in _components)
			{
				tuple = component.ModifyCardPlayResultPileTypeAndPosition(card, isAutoPlay, resources, tuple.Item1, tuple.Item2);
			}
			return ModifyCardPlayResultPileTypeAndPositionC(card, isAutoPlay, resources, tuple.Item1, tuple.Item2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
		{
			EnsureComponentsInitialized();
			int triggerCount2 = triggerCount;
			foreach (ICardComponent component in _components)
			{
				triggerCount2 = component.ModifyOrbPassiveTriggerCounts(orb, triggerCount2);
			}
			return ModifyOrbPassiveTriggerCountsC(orb, triggerCount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
		{
			EnsureComponentsInitialized();
			CardCreationOptions options2 = options;
			foreach (ICardComponent component in _components)
			{
				options2 = component.ModifyCardRewardCreationOptions(player, options2);
			}
			return ModifyCardRewardCreationOptionsC(player, options2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override CardCreationOptions ModifyCardRewardCreationOptionsLate(Player player, CardCreationOptions options)
		{
			EnsureComponentsInitialized();
			CardCreationOptions options2 = options;
			foreach (ICardComponent component in _components)
			{
				options2 = component.ModifyCardRewardCreationOptionsLate(player, options2);
			}
			return ModifyCardRewardCreationOptionsLateC(player, options2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
		{
			EnsureComponentsInitialized();
			decimal odds2 = odds;
			foreach (ICardComponent component in _components)
			{
				odds2 = component.ModifyCardRewardUpgradeOdds(player, card, odds2);
			}
			return ModifyCardRewardUpgradeOddsC(player, card, odds2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyDamageAdditive(target, amount2, props, dealer, cardSource);
			}
			return ModifyDamageAdditiveC(target, amount2, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			foreach (ICardComponent component in _components)
			{
				component.ModifyDamageCap(target, props, dealer, cardSource);
			}
			return ModifyDamageCapC(target, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyDamageMultiplicative(target, amount2, props, dealer, cardSource);
			}
			return ModifyDamageMultiplicativeC(target, amount2, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyEnergyGain(Player player, decimal amount)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyEnergyGain(player, amount2);
			}
			return ModifyEnergyGainC(player, amount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyGoldGained(Player player, decimal amount)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyGoldGained(player, amount2);
			}
			return ModifyGoldGainedC(player, amount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
		{
			EnsureComponentsInitialized();
			ActMap map2 = map;
			foreach (ICardComponent component in _components)
			{
				map2 = component.ModifyGeneratedMap(runState, map2, actIndex);
			}
			return ModifyGeneratedMapC(runState, map2, actIndex);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
		{
			EnsureComponentsInitialized();
			ActMap map2 = map;
			foreach (ICardComponent component in _components)
			{
				map2 = component.ModifyGeneratedMapLate(runState, map2, actIndex);
			}
			return ModifyGeneratedMapLateC(runState, map2, actIndex);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyHandDraw(Player player, decimal count)
		{
			EnsureComponentsInitialized();
			decimal count2 = count;
			foreach (ICardComponent component in _components)
			{
				count2 = component.ModifyHandDraw(player, count2);
			}
			return ModifyHandDrawC(player, count2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyHandDrawLate(Player player, decimal count)
		{
			EnsureComponentsInitialized();
			decimal count2 = count;
			foreach (ICardComponent component in _components)
			{
				count2 = component.ModifyHandDrawLate(player, count2);
			}
			return ModifyHandDrawLateC(player, count2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyHpLostBeforeOsty(target, amount2, props, dealer, cardSource);
			}
			return ModifyHpLostBeforeOstyC(target, amount2, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyHpLostBeforeOstyLate(target, amount2, props, dealer, cardSource);
			}
			return ModifyHpLostBeforeOstyLateC(target, amount2, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyHpLostAfterOsty(target, amount2, props, dealer, cardSource);
			}
			return ModifyHpLostAfterOstyC(target, amount2, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyHpLostAfterOstyLate(target, amount2, props, dealer, cardSource);
			}
			return ModifyHpLostAfterOstyLateC(target, amount2, props, dealer, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyMaxEnergy(Player player, decimal amount)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyMaxEnergy(player, amount2);
			}
			return ModifyMaxEnergyC(player, amount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
		{
			EnsureComponentsInitialized();
			IEnumerable<CardModel> options2 = options;
			foreach (ICardComponent component in _components)
			{
				options2 = component.ModifyMerchantCardPool(player, options2);
			}
			return ModifyMerchantCardPoolC(player, options2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override CardRarity ModifyMerchantCardRarity(Player player, CardRarity rarity)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			CardRarity rarity2 = rarity;
			foreach (ICardComponent component in _components)
			{
				rarity2 = component.ModifyMerchantCardRarity(player, rarity2);
			}
			return ModifyMerchantCardRarityC(player, rarity2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
		{
			EnsureComponentsInitialized();
			foreach (ICardComponent component in _components)
			{
				component.ModifyMerchantCardCreationResults(player, cards);
			}
			ModifyMerchantCardCreationResultsC(player, cards);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
		{
			EnsureComponentsInitialized();
			decimal cost2 = cost;
			foreach (ICardComponent component in _components)
			{
				cost2 = component.ModifyMerchantPrice(player, entry, cost2);
			}
			return ModifyMerchantPriceC(player, entry, cost2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyOrbValue(OrbModel orb, decimal value)
		{
			EnsureComponentsInitialized();
			decimal value2 = value;
			foreach (ICardComponent component in _components)
			{
				value2 = component.ModifyOrbValue(orb, value2);
			}
			return ModifyOrbValueC(orb, value2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyPowerAmountGivenAdditive(power, giver, amount2, target, cardSource);
			}
			return ModifyPowerAmountGivenAdditiveC(power, giver, amount2, target, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyPowerAmountGivenMultiplicative(power, giver, amount2, target, cardSource);
			}
			return ModifyPowerAmountGivenMultiplicativeC(power, giver, amount2, target, cardSource);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifyRestSiteHealAmount(creature, amount2);
			}
			return ModifyRestSiteHealAmountC(creature, amount2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
		{
			EnsureComponentsInitialized();
			foreach (ICardComponent component in _components)
			{
				component.ModifyShuffleOrder(player, cards, isInitialShuffle);
			}
			ModifyShuffleOrderC(player, cards, isInitialShuffle);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override decimal ModifySummonAmount(Player summoner, decimal amount, AbstractModel? source)
		{
			EnsureComponentsInitialized();
			decimal amount2 = amount;
			foreach (ICardComponent component in _components)
			{
				amount2 = component.ModifySummonAmount(summoner, amount2, source);
			}
			return ModifySummonAmountC(summoner, amount2, source);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
		{
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			Creature target2 = target;
			foreach (ICardComponent component in _components)
			{
				target2 = component.ModifyUnblockedDamageTarget(target2, amount, props, dealer);
			}
			return ModifyUnblockedDamageTargetC(target2, amount, props, dealer);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override EventModel ModifyNextEvent(EventModel currentEvent)
		{
			EnsureComponentsInitialized();
			EventModel currentEvent2 = currentEvent;
			foreach (ICardComponent component in _components)
			{
				currentEvent2 = component.ModifyNextEvent(currentEvent2);
			}
			return ModifyNextEventC(currentEvent2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
		{
			EnsureComponentsInitialized();
			IReadOnlySet<RoomType> roomTypes2 = roomTypes;
			foreach (ICardComponent component in _components)
			{
				roomTypes2 = component.ModifyUnknownMapPointRoomTypes(roomTypes2);
			}
			return ModifyUnknownMapPointRoomTypesC(roomTypes2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			float oddsIncrease2 = oddsIncrease;
			foreach (ICardComponent component in _components)
			{
				oddsIncrease2 = component.ModifyOddsIncreaseForUnrolledRoomType(roomType, oddsIncrease2);
			}
			return ModifyOddsIncreaseForUnrolledRoomTypeC(roomType, oddsIncrease2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override int ModifyXValue(CardModel card, int originalValue)
		{
			EnsureComponentsInitialized();
			int originalValue2 = originalValue;
			foreach (ICardComponent component in _components)
			{
				originalValue2 = component.ModifyXValue(card, originalValue2);
			}
			return ModifyXValueC(card, originalValue2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				if (component.TryModifyCardBeingAddedToDeck(card, out CardModel newCard2) && newCard2 != null)
				{
					flag = true;
					card = newCard2;
				}
			}
			if (TryModifyCardBeingAddedToDeckC(card, out CardModel newCard3) && newCard3 != null)
			{
				flag = true;
				card = newCard3;
			}
			newCard = (flag ? card : null);
			return flag;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyCardBeingAddedToDeckLate(CardModel card, out CardModel? newCard)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				if (component.TryModifyCardBeingAddedToDeckLate(card, out CardModel newCard2) && newCard2 != null)
				{
					flag = true;
					card = newCard2;
				}
			}
			if (TryModifyCardBeingAddedToDeckLateC(card, out CardModel newCard3) && newCard3 != null)
			{
				flag = true;
				card = newCard3;
			}
			newCard = (flag ? card : null);
			return flag;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyCardRewardAlternatives(player, cardReward, alternatives);
			}
			return flag | TryModifyCardRewardAlternativesC(player, cardReward, alternatives);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyCardRewardOptions(player, cardRewardOptions, creationOptions);
			}
			return flag | TryModifyCardRewardOptionsC(player, cardRewardOptions, creationOptions);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyCardRewardOptionsLate(player, cardRewardOptions, creationOptions);
			}
			return flag | TryModifyCardRewardOptionsLateC(player, cardRewardOptions, creationOptions);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			decimal num = originalCost;
			foreach (ICardComponent component in _components)
			{
				if (component.TryModifyEnergyCostInCombat(card, num, out var modifiedCost2))
				{
					flag = true;
					num = modifiedCost2;
				}
			}
			if (TryModifyEnergyCostInCombatC(card, num, out var modifiedCost3))
			{
				flag = true;
				num = modifiedCost3;
			}
			modifiedCost = (flag ? num : originalCost);
			return flag;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			decimal num = originalCost;
			foreach (ICardComponent component in _components)
			{
				if (component.TryModifyStarCost(card, num, out var modifiedCost2))
				{
					flag = true;
					num = modifiedCost2;
				}
			}
			if (TryModifyStarCostC(card, num, out var modifiedCost3))
			{
				flag = true;
				num = modifiedCost3;
			}
			modifiedCost = (flag ? num : originalCost);
			return flag;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			decimal num = amount;
			foreach (ICardComponent component in _components)
			{
				if (component.TryModifyPowerAmountReceived(canonicalPower, target, num, applier, out var modifiedAmount2))
				{
					flag = true;
					num = modifiedAmount2;
				}
			}
			if (TryModifyPowerAmountReceivedC(canonicalPower, target, num, applier, out var modifiedAmount3))
			{
				flag = true;
				num = modifiedAmount3;
			}
			modifiedAmount = (flag ? num : amount);
			return flag;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyRestSiteOptions(player, options);
			}
			return flag | TryModifyRestSiteOptionsC(player, options);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyRestSiteHealRewards(player, rewards, isMimicked);
			}
			return flag | TryModifyRestSiteHealRewardsC(player, rewards, isMimicked);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyRewards(player, rewards, room);
			}
			return flag | TryModifyRewardsC(player, rewards, room);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			foreach (ICardComponent component in _components)
			{
				flag |= component.TryModifyRewardsLate(player, rewards, room);
			}
			return flag | TryModifyRewardsLateC(player, rewards, room);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
		{
			EnsureComponentsInitialized();
			IReadOnlyList<LocString> currentExtraText2 = currentExtraText;
			foreach (ICardComponent component in _components)
			{
				currentExtraText2 = component.ModifyExtraRestSiteHealText(player, currentExtraText2);
			}
			return ModifyExtraRestSiteHealTextC(player, currentExtraText2);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool TryModifyEnergyCostInCombatLate(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			EnsureComponentsInitialized();
			bool flag = false;
			decimal num = originalCost;
			foreach (ICardComponent component in _components)
			{
				if (component.TryModifyEnergyCostInCombatLate(card, num, out var modifiedCost2))
				{
					flag = true;
					num = modifiedCost2;
				}
			}
			if (TryModifyEnergyCostInCombatLateC(card, num, out var modifiedCost3))
			{
				flag = true;
				num = modifiedCost3;
			}
			modifiedCost = (flag ? num : originalCost);
			return flag;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAddToDeck(CardModel card)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldAddToDeck(card)))
			{
				return ShouldAddToDeckC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAfflict(CardModel card, AfflictionModel affliction)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldAfflict(card, affliction)))
			{
				return ShouldAfflictC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAllowAncient(Player player, AncientEventModel ancient)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldAllowAncient(player, ancient)))
			{
				return ShouldAllowAncientC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAllowHitting(Creature creature)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldAllowHitting(creature)))
			{
				return ShouldAllowHittingC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAllowTargeting(Creature target)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldAllowTargeting(target)))
			{
				return ShouldAllowTargetingC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.ShouldAllowSelectingMoreCardRewards(player, cardReward)))
			{
				return ShouldAllowSelectingMoreCardRewardsC;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldClearBlock(Creature creature)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldClearBlock(creature)))
			{
				return ShouldClearBlockC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldDie(Creature creature)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldDie(creature)))
			{
				return ShouldDieC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldDieLate(Creature creature)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldDieLate(creature)))
			{
				return ShouldDieLateC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldDisableRemainingRestSiteOptions(Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldDisableRemainingRestSiteOptions(player)))
			{
				return ShouldDisableRemainingRestSiteOptionsC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldDraw(Player player, bool fromHandDraw)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldDraw(player, fromHandDraw)))
			{
				return ShouldDrawC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldEtherealTrigger(CardModel card)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldEtherealTrigger(card)))
			{
				return ShouldEtherealTriggerC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldFlush(Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldFlush(player)))
			{
				return ShouldFlushC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldGainStars(decimal amount, Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldGainStars(amount, player)))
			{
				return ShouldGainStarsC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldGenerateTreasure(Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldGenerateTreasure(player)))
			{
				return ShouldGenerateTreasureC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldPayExcessEnergyCostWithStars(Player player)
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.ShouldPayExcessEnergyCostWithStars(player)))
			{
				return ShouldPayExcessEnergyCostWithStarsC;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			if (_components.All(delegate(ICardComponent c)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				return c.ShouldPlay(card, autoPlayType);
			}))
			{
				return ShouldPlayC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldPlayerResetEnergy(Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldPlayerResetEnergy(player)))
			{
				return ShouldPlayerResetEnergyC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldProceedToNextMapPoint()
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldProceedToNextMapPoint()))
			{
				return ShouldProceedToNextMapPointC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldProcurePotion(PotionModel potion, Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldProcurePotion(potion, player)))
			{
				return ShouldProcurePotionC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldPowerBeRemovedOnDeath(PowerModel power)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldPowerBeRemovedOnDeath(power)))
			{
				return ShouldPowerBeRemovedOnDeathC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldRefillMerchantEntry(MerchantEntry entry, Player player)
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.ShouldRefillMerchantEntry(entry, player)))
			{
				return ShouldRefillMerchantEntryC;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAllowMerchantCardRemoval(Player player)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldAllowMerchantCardRemoval(player)))
			{
				return ShouldAllowMerchantCardRemovalC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
		{
			EnsureComponentsInitialized();
			if (_components.All((ICardComponent c) => c.ShouldCreatureBeRemovedFromCombatAfterDeath(creature)))
			{
				return ShouldCreatureBeRemovedFromCombatAfterDeathC;
			}
			return false;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldStopCombatFromEnding()
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.ShouldStopCombatFromEnding()))
			{
				return ShouldStopCombatFromEndingC;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldTakeExtraTurn(Player player)
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.ShouldTakeExtraTurn(player)))
			{
				return ShouldTakeExtraTurnC;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldForcePotionReward(Player player, RoomType roomType)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			EnsureComponentsInitialized();
			if (!_components.Any(delegate(ICardComponent c)
			{
				//IL_0008: Unknown result type (might be due to invalid IL or missing references)
				return c.ShouldForcePotionReward(player, roomType);
			}))
			{
				return ShouldForcePotionRewardC;
			}
			return true;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("This member is sealed. Try using the C-ending one instead, or disable this warning if intended.", false)]
		public sealed override bool ShouldAllowFreeTravel()
		{
			EnsureComponentsInitialized();
			if (!_components.Any((ICardComponent c) => c.ShouldAllowFreeTravel()))
			{
				return ShouldAllowFreeTravelC;
			}
			return true;
		}

		protected virtual int ModifyAttackHitCountC(AttackCommand attack, int hitCount)
		{
			return hitCount;
		}

		protected virtual decimal ModifyBlockAdditiveC(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			return 0m;
		}

		protected virtual decimal ModifyBlockMultiplicativeC(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			return 1m;
		}

		protected virtual int ModifyCardPlayCountC(CardModel card, Creature? target, int playCount)
		{
			return playCount;
		}

		protected virtual (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPositionC(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return (pileType, position);
		}

		protected virtual int ModifyOrbPassiveTriggerCountsC(OrbModel orb, int triggerCount)
		{
			return triggerCount;
		}

		protected virtual CardCreationOptions ModifyCardRewardCreationOptionsC(Player player, CardCreationOptions options)
		{
			return options;
		}

		protected virtual CardCreationOptions ModifyCardRewardCreationOptionsLateC(Player player, CardCreationOptions options)
		{
			return options;
		}

		protected virtual decimal ModifyCardRewardUpgradeOddsC(Player player, CardModel card, decimal odds)
		{
			return odds;
		}

		protected virtual decimal ModifyDamageAdditiveC(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return 0m;
		}

		protected virtual decimal ModifyDamageCapC(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return decimal.MaxValue;
		}

		protected virtual decimal ModifyDamageMultiplicativeC(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return 1m;
		}

		protected virtual decimal ModifyEnergyGainC(Player player, decimal amount)
		{
			return amount;
		}

		protected virtual decimal ModifyGoldGainedC(Player player, decimal amount)
		{
			return amount;
		}

		protected virtual ActMap ModifyGeneratedMapC(IRunState runState, ActMap map, int actIndex)
		{
			return map;
		}

		protected virtual ActMap ModifyGeneratedMapLateC(IRunState runState, ActMap map, int actIndex)
		{
			return map;
		}

		protected virtual decimal ModifyHandDrawC(Player player, decimal count)
		{
			return count;
		}

		protected virtual decimal ModifyHandDrawLateC(Player player, decimal count)
		{
			return count;
		}

		protected virtual decimal ModifyHpLostBeforeOstyC(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		protected virtual decimal ModifyHpLostBeforeOstyLateC(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		protected virtual decimal ModifyHpLostAfterOstyC(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		protected virtual decimal ModifyHpLostAfterOstyLateC(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		protected virtual decimal ModifyMaxEnergyC(Player player, decimal amount)
		{
			return amount;
		}

		protected virtual IEnumerable<CardModel> ModifyMerchantCardPoolC(Player player, IEnumerable<CardModel> options)
		{
			return options;
		}

		protected virtual CardRarity ModifyMerchantCardRarityC(Player player, CardRarity rarity)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return rarity;
		}

		protected virtual void ModifyMerchantCardCreationResultsC(Player player, List<CardCreationResult> cards)
		{
		}

		protected virtual decimal ModifyMerchantPriceC(Player player, MerchantEntry entry, decimal cost)
		{
			return cost;
		}

		protected virtual decimal ModifyOrbValueC(OrbModel orb, decimal value)
		{
			return value;
		}

		protected virtual decimal ModifyPowerAmountGivenAdditiveC(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			return 0m;
		}

		protected virtual decimal ModifyPowerAmountGivenMultiplicativeC(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			return 1m;
		}

		protected virtual decimal ModifyRestSiteHealAmountC(Creature creature, decimal amount)
		{
			return amount;
		}

		protected virtual void ModifyShuffleOrderC(Player player, List<CardModel> cards, bool isInitialShuffle)
		{
		}

		protected virtual decimal ModifySummonAmountC(Player summoner, decimal amount, AbstractModel? source)
		{
			return amount;
		}

		protected virtual Creature ModifyUnblockedDamageTargetC(Creature target, decimal amount, ValueProp props, Creature? dealer)
		{
			return target;
		}

		protected virtual EventModel ModifyNextEventC(EventModel currentEvent)
		{
			return currentEvent;
		}

		protected virtual IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypesC(IReadOnlySet<RoomType> roomTypes)
		{
			return roomTypes;
		}

		protected virtual float ModifyOddsIncreaseForUnrolledRoomTypeC(RoomType roomType, float oddsIncrease)
		{
			return oddsIncrease;
		}

		protected virtual int ModifyXValueC(CardModel card, int originalValue)
		{
			return originalValue;
		}

		protected virtual bool TryModifyCardBeingAddedToDeckC(CardModel card, out CardModel? newCard)
		{
			newCard = null;
			return false;
		}

		protected virtual bool TryModifyCardBeingAddedToDeckLateC(CardModel card, out CardModel? newCard)
		{
			newCard = null;
			return false;
		}

		protected virtual bool TryModifyCardRewardAlternativesC(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
		{
			return false;
		}

		protected virtual bool TryModifyCardRewardOptionsC(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			return false;
		}

		protected virtual bool TryModifyCardRewardOptionsLateC(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			return false;
		}

		protected virtual bool TryModifyEnergyCostInCombatC(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			return false;
		}

		protected virtual bool TryModifyStarCostC(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			return false;
		}

		protected virtual bool TryModifyPowerAmountReceivedC(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
		{
			modifiedAmount = amount;
			return false;
		}

		protected virtual bool TryModifyRestSiteOptionsC(Player player, ICollection<RestSiteOption> options)
		{
			return false;
		}

		protected virtual bool TryModifyRestSiteHealRewardsC(Player player, List<Reward> rewards, bool isMimicked)
		{
			return false;
		}

		protected virtual bool TryModifyRewardsC(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			return false;
		}

		protected virtual bool TryModifyRewardsLateC(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			return false;
		}

		protected virtual IReadOnlyList<LocString> ModifyExtraRestSiteHealTextC(Player player, IReadOnlyList<LocString> currentExtraText)
		{
			return currentExtraText;
		}

		protected virtual bool TryModifyEnergyCostInCombatLateC(CardModel card, decimal currentCost, out decimal modifiedCost)
		{
			modifiedCost = currentCost;
			return false;
		}
	}
}
namespace MinionLib.Component.Utils
{
	public abstract class AmountCardComponent : CardComponent
	{
		private decimal __amountBackingField;

		[ComponentState<DynamicVar>(new object[] { })]
		public decimal Amount
		{
			get
			{
				return __amountBackingField;
			}
			set
			{
				__amountBackingField = value;
				base.DynamicVars["Amount"].BaseValue = Convert.ToDecimal(value);
			}
		}

		protected override IEnumerable<DynamicVar> SmartVars
		{
			get
			{
				//IL_0010: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Expected O, but got Unknown
				return new <>z__ReadOnlySingleElementList<DynamicVar>(new DynamicVar("Amount", Convert.ToDecimal(Amount)));
			}
		}

		public override bool TryMergeWith(ICardComponent incoming, ApplyComponentOptions options, out ICardComponent? merged)
		{
			if (!(incoming is AmountCardComponent amountCardComponent))
			{
				merged = null;
				return false;
			}
			Amount += amountCardComponent.Amount;
			if (options.IsUpgrade)
			{
				foreach (KeyValuePair<string, DynamicVar> dynamicVar in base.DynamicVars)
				{
					dynamicVar.Value.SetWasJustUpgraded();
				}
			}
			merged = ((Amount == 0m) ? null : this);
			return true;
		}

		public override bool TrySubtractiveMergeWith(ICardComponent incoming, ApplyComponentOptions options, out ICardComponent? merged)
		{
			if (!(incoming is AmountCardComponent amountCardComponent))
			{
				merged = null;
				return false;
			}
			Amount -= amountCardComponent.Amount;
			if (options.IsUpgrade)
			{
				foreach (KeyValuePair<string, DynamicVar> dynamicVar in base.DynamicVars)
				{
					dynamicVar.Value.SetWasJustUpgraded();
				}
			}
			merged = ((Amount == 0m) ? null : this);
			return true;
		}

		public override void Serialize(ArrayBufferWriter<byte> writer)
		{
			base.Serialize(writer);
			SerializationUtils.WriteDecimal(writer, Amount);
		}

		public override bool Deserialize(ref ReadOnlySpan<byte> reader)
		{
			if (!base.Deserialize(ref reader))
			{
				return false;
			}
			if (!SerializationUtils.TryReadDecimal(ref reader, out var value))
			{
				return false;
			}
			Amount = value;
			return true;
		}
	}
	public enum Timing
	{
		OnPlay,
		OnEnqueuePlayVfx,
		OnTurnEndInHand,
		BeforeCardPlayed,
		AfterCardPlayed,
		AfterCardPlayedLate,
		AfterPlayerTurnStartEarly,
		AfterPlayerTurnStart,
		AfterPlayerTurnStartLate,
		AfterAutoPostPlayPhaseEntered,
		AfterAutoPrePlayPhaseEnteredEarly,
		AfterAutoPrePlayPhaseEntered,
		AfterAutoPrePlayPhaseEnteredLate,
		BeforeSideTurnEndVeryEarly,
		BeforeSideTurnEndEarly,
		BeforeSideTurnEnd,
		AfterSideTurnEnd,
		AfterSideTurnEndLate,
		AfterActEntered,
		AfterAddToDeckPrevented,
		BeforeAttack,
		AfterAttack,
		AfterBlockCleared,
		BeforeBlockGained,
		AfterBlockGained,
		AfterBlockBroken,
		AfterCardChangedPiles,
		AfterCardChangedPilesLate,
		AfterCardDiscarded,
		AfterCardDrawnEarly,
		AfterCardDrawn,
		AfterCardEnteredCombat,
		AfterCardGeneratedForCombat,
		AfterCardExhausted,
		BeforeCardAutoPlayed,
		BeforeCombatStart,
		BeforeCombatStartLate,
		AfterCombatEnd,
		AfterCombatVictoryEarly,
		AfterCombatVictory,
		AfterCreatureAddedToCombat,
		AfterCurrentHpChanged,
		AfterDamageGiven,
		BeforeDamageReceived,
		AfterDamageReceived,
		AfterDamageReceivedLate,
		BeforeDeath,
		AfterDeath,
		AfterDiedToDoom,
		AfterEnergyReset,
		AfterEnergyResetLate,
		AfterEnergySpent,
		BeforeCardRemoved,
		BeforeFlush,
		BeforeFlushLate,
		AfterFlush,
		AfterGoldGained,
		BeforeHandDraw,
		BeforeHandDrawLate,
		AfterHandEmptied,
		AfterItemPurchased,
		AfterMapGenerated,
		AfterModifyingBlockAmount,
		AfterModifyingCardPlayCount,
		AfterModifyingCardPlayResultPileOrPosition,
		AfterModifyingOrbPassiveTriggerCount,
		AfterModifyingCardRewardOptions,
		AfterModifyingDamageAmount,
		AfterModifyingEnergyGain,
		AfterModifyingHandDraw,
		AfterPreventingDraw,
		AfterModifyingHpLostBeforeOsty,
		AfterModifyingHpLostAfterOsty,
		AfterModifyingPowerAmountReceived,
		AfterModifyingPowerAmountGiven,
		AfterModifyingRewards,
		AfterOrbChanneled,
		AfterOrbEvoked,
		AfterOstyRevived,
		BeforePotionUsed,
		AfterPotionUsed,
		AfterPotionDiscarded,
		AfterPotionProcured,
		BeforePowerAmountChanged,
		AfterPowerAmountChanged,
		AfterPreventingBlockClear,
		AfterPreventingDeath,
		AfterRestSiteHeal,
		AfterRestSiteSmith,
		AfterRewardTaken,
		BeforeRoomEntered,
		AfterRoomEntered,
		AfterShuffle,
		AfterStarsSpent,
		AfterStarsGained,
		AfterForge,
		AfterSummon,
		AfterTakingExtraTurn,
		AfterTargetingBlockedVfx,
		BeforeSideTurnStart,
		AfterSideTurnStart,
		AfterSideTurnStartLate,
		AfterModifyingGoldGained,
		AfterTransformedFrom,
		AfterTransformedTo
	}
	public abstract class TimingCardComponent(params Timing[] timings) : CardComponent
	{
		[ComponentState]
		protected Timing[] Timings { get; set; } = timings;

		protected virtual Task OnTimingPrefix(OnTimingContext context)
		{
			return Task.CompletedTask;
		}

		protected virtual Task OnTimingPostfix(OnTimingContext context)
		{
			return Task.CompletedTask;
		}

		public override Task OnPlayPrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.OnPlay))
			{
				OnTimingContext context = new OnTimingContext(Timing.OnPlay, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task OnPlayPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.OnPlay))
			{
				OnTimingContext context = new OnTimingContext(Timing.OnPlay, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task OnEnqueuePlayVfxPrefix(Creature? target, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.OnEnqueuePlayVfx))
			{
				OnTimingContext context = new OnTimingContext(Timing.OnEnqueuePlayVfx, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task OnEnqueuePlayVfxPostfix(Creature? target, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.OnEnqueuePlayVfx))
			{
				OnTimingContext context = new OnTimingContext(Timing.OnEnqueuePlayVfx, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task OnTurnEndInHandPrefix(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.OnTurnEndInHand))
			{
				OnTimingContext context = new OnTimingContext(Timing.OnTurnEndInHand, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task OnTurnEndInHandPostfix(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.OnTurnEndInHand))
			{
				OnTimingContext context = new OnTimingContext(Timing.OnTurnEndInHand, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCardPlayedPrefix(CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCardPlayed))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCardPlayed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCardPlayedPostfix(CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCardPlayed))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCardPlayed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardPlayedPrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardPlayed))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardPlayed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardPlayedPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardPlayed))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardPlayed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardPlayedLatePrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardPlayedLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardPlayedLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardPlayedLatePostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardPlayedLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardPlayedLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPlayerTurnStartEarlyPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPlayerTurnStartEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPlayerTurnStartEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPlayerTurnStartEarlyPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPlayerTurnStartEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPlayerTurnStartEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPlayerTurnStartPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPlayerTurnStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPlayerTurnStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPlayerTurnStartPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPlayerTurnStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPlayerTurnStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPlayerTurnStartLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPlayerTurnStartLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPlayerTurnStartLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPlayerTurnStartLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPlayerTurnStartLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPlayerTurnStartLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPostPlayPhaseEnteredPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPostPlayPhaseEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPostPlayPhaseEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPostPlayPhaseEnteredPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPostPlayPhaseEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPostPlayPhaseEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPrePlayPhaseEnteredEarlyPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPrePlayPhaseEnteredEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPrePlayPhaseEnteredEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPrePlayPhaseEnteredEarlyPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPrePlayPhaseEnteredEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPrePlayPhaseEnteredEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPrePlayPhaseEnteredPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPrePlayPhaseEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPrePlayPhaseEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPrePlayPhaseEnteredPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPrePlayPhaseEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPrePlayPhaseEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPrePlayPhaseEnteredLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPrePlayPhaseEnteredLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPrePlayPhaseEnteredLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAutoPrePlayPhaseEnteredLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAutoPrePlayPhaseEnteredLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAutoPrePlayPhaseEnteredLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnEndVeryEarlyPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnEndVeryEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnEndVeryEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnEndVeryEarlyPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnEndVeryEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnEndVeryEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnEndEarlyPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnEndEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnEndEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnEndEarlyPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnEndEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnEndEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnEndPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnEnd))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnEnd, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnEndPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnEnd))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnEnd, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnEndPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnEnd))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnEnd, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnEndPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnEnd))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnEnd, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnEndLatePrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnEndLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnEndLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnEndLatePostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnEndLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnEndLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterActEnteredPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterActEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterActEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterActEnteredPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterActEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterActEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAddToDeckPreventedPrefix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAddToDeckPrevented))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAddToDeckPrevented, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAddToDeckPreventedPostfix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAddToDeckPrevented))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAddToDeckPrevented, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeAttackPrefix(AttackCommand command, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeAttack))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeAttack, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, command);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeAttackPostfix(AttackCommand command, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeAttack))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeAttack, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, command);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAttackPrefix(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAttack))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAttack, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, command);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterAttackPostfix(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterAttack))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterAttack, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, command);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterBlockClearedPrefix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterBlockCleared))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterBlockCleared, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterBlockClearedPostfix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterBlockCleared))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterBlockCleared, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeBlockGainedPrefix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeBlockGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeBlockGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, null, CausedByEthereal: false, 0f, null, null, creature, null, null, amount);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeBlockGainedPostfix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeBlockGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeBlockGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, null, CausedByEthereal: false, 0f, null, null, creature, null, null, amount);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterBlockGainedPrefix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterBlockGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterBlockGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, null, CausedByEthereal: false, 0f, null, null, creature, null, null, amount);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterBlockGainedPostfix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterBlockGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterBlockGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, null, CausedByEthereal: false, 0f, null, null, creature, null, null, amount);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterBlockBrokenPrefix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterBlockBroken))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterBlockBroken, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterBlockBrokenPostfix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterBlockBroken))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterBlockBroken, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardChangedPilesPrefix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterCardChangedPiles))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardChangedPiles, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, oldPileType, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, source);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardChangedPilesPostfix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterCardChangedPiles))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardChangedPiles, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, oldPileType, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, source);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardChangedPilesLatePrefix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterCardChangedPilesLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardChangedPilesLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, oldPileType, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, source);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardChangedPilesLatePostfix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterCardChangedPilesLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardChangedPilesLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, oldPileType, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, source);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardDiscardedPrefix(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardDiscarded))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardDiscarded, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardDiscardedPostfix(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardDiscarded))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardDiscarded, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardDrawnEarlyPrefix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardDrawnEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardDrawnEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, fromHandDraw, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardDrawnEarlyPostfix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardDrawnEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardDrawnEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, fromHandDraw, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardDrawnPrefix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardDrawn))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardDrawn, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, fromHandDraw, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardDrawnPostfix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardDrawn))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardDrawn, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, fromHandDraw, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardEnteredCombatPrefix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardEnteredCombat))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardEnteredCombat, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardEnteredCombatPostfix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardEnteredCombat))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardEnteredCombat, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardGeneratedForCombatPrefix(CardModel card, Player? creator, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardGeneratedForCombat))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardGeneratedForCombat, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, creator, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardGeneratedForCombatPostfix(CardModel card, Player? creator, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardGeneratedForCombat))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardGeneratedForCombat, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, creator, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardExhaustedPrefix(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardExhausted))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardExhausted, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, causedByEthereal);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCardExhaustedPostfix(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCardExhausted))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCardExhausted, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, causedByEthereal);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCardAutoPlayedPrefix(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeCardAutoPlayed))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCardAutoPlayed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, type, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCardAutoPlayedPostfix(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeCardAutoPlayed))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCardAutoPlayed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, type, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCombatStartPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCombatStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCombatStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCombatStartPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCombatStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCombatStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCombatStartLatePrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCombatStartLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCombatStartLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCombatStartLatePostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCombatStartLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCombatStartLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatEndPrefix(CombatRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCombatEnd))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCombatEnd, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, (AbstractRoom)(object)room);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatEndPostfix(CombatRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCombatEnd))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCombatEnd, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, (AbstractRoom)(object)room);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatVictoryEarlyPrefix(CombatRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCombatVictoryEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCombatVictoryEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, (AbstractRoom)(object)room);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatVictoryEarlyPostfix(CombatRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCombatVictoryEarly))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCombatVictoryEarly, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, (AbstractRoom)(object)room);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatVictoryPrefix(CombatRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCombatVictory))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCombatVictory, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, (AbstractRoom)(object)room);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCombatVictoryPostfix(CombatRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCombatVictory))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCombatVictory, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, (AbstractRoom)(object)room);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCreatureAddedToCombatPrefix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCreatureAddedToCombat))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCreatureAddedToCombat, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCreatureAddedToCombatPostfix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCreatureAddedToCombat))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCreatureAddedToCombat, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCurrentHpChangedPrefix(Creature creature, decimal delta, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCurrentHpChanged))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCurrentHpChanged, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, delta, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterCurrentHpChangedPostfix(Creature creature, decimal delta, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterCurrentHpChanged))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterCurrentHpChanged, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, delta, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDamageGivenPrefix(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterDamageGiven))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDamageGiven, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, result, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDamageGivenPostfix(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterDamageGiven))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDamageGiven, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, result, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeDamageReceivedPrefix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeDamageReceived))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeDamageReceived, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeDamageReceivedPostfix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeDamageReceived))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeDamageReceived, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDamageReceivedPrefix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterDamageReceived))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDamageReceived, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, result, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDamageReceivedPostfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterDamageReceived))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDamageReceived, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, result, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDamageReceivedLatePrefix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterDamageReceivedLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDamageReceivedLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, result, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDamageReceivedLatePostfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterDamageReceivedLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDamageReceivedLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, dealer, 0m, null, (CombatSide)0, IsMimicked: false, null, props, 0m, result, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeDeathPrefix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeDeath))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeDeath, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeDeathPostfix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeDeath))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeDeath, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDeathPrefix(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterDeath))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDeath, 0, null, null, null, wasRemovalPrevented, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, deathAnimLength, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDeathPostfix(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterDeath))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDeath, 0, null, null, null, wasRemovalPrevented, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, deathAnimLength, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDiedToDoomPrefix(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterDiedToDoom))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDiedToDoom, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, creatures);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterDiedToDoomPostfix(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterDiedToDoom))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterDiedToDoom, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, creatures);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterEnergyResetPrefix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterEnergyReset))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterEnergyReset, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterEnergyResetPostfix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterEnergyReset))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterEnergyReset, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterEnergyResetLatePrefix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterEnergyResetLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterEnergyResetLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterEnergyResetLatePostfix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterEnergyResetLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterEnergyResetLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterEnergySpentPrefix(CardModel card, int amount, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterEnergySpent))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterEnergySpent, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterEnergySpentPostfix(CardModel card, int amount, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterEnergySpent))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterEnergySpent, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCardRemovedPrefix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCardRemoved))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCardRemoved, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeCardRemovedPostfix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeCardRemoved))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeCardRemoved, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeFlushPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeFlush))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeFlush, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeFlushPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeFlush))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeFlush, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeFlushLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeFlushLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeFlushLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeFlushLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeFlushLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeFlushLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterFlushPrefix(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterFlush))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterFlush, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, flushedCards, retainedCards);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterFlushPostfix(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterFlush))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterFlush, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, 0, null, flushedCards, retainedCards);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterGoldGainedPrefix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterGoldGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterGoldGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterGoldGainedPostfix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterGoldGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterGoldGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeHandDrawPrefix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeHandDraw))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeHandDraw, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeHandDrawPostfix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeHandDraw))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeHandDraw, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeHandDrawLatePrefix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeHandDrawLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeHandDrawLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeHandDrawLatePostfix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeHandDrawLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeHandDrawLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterHandEmptiedPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterHandEmptied))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterHandEmptied, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterHandEmptiedPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterHandEmptied))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterHandEmptied, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterItemPurchasedPrefix(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterItemPurchased))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterItemPurchased, 0, null, null, null, WasRemovalPrevented: false, itemPurchased, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, goldSpent);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterItemPurchasedPostfix(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterItemPurchased))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterItemPurchased, 0, null, null, null, WasRemovalPrevented: false, itemPurchased, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, null, null, null, null, goldSpent);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterMapGeneratedPrefix(ActMap map, int actIndex, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterMapGenerated))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterMapGenerated, actIndex, null, null, map, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterMapGeneratedPostfix(ActMap map, int actIndex, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterMapGenerated))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterMapGenerated, actIndex, null, null, map, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingBlockAmountPrefix(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingBlockAmount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingBlockAmount, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, modifiedAmount, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingBlockAmountPostfix(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingBlockAmount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingBlockAmount, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, modifiedAmount, null, CausedByEthereal: false, 0f, cardPlay);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingCardPlayCountPrefix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingCardPlayCount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingCardPlayCount, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingCardPlayCountPostfix(CardModel card, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingCardPlayCount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingCardPlayCount, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingCardPlayResultPileOrPositionPrefix(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterModifyingCardPlayResultPileOrPosition))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingCardPlayResultPileOrPosition, 0, null, null, null, WasRemovalPrevented: false, null, null, position, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, pileType, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingCardPlayResultPileOrPositionPostfix(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterModifyingCardPlayResultPileOrPosition))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingCardPlayResultPileOrPosition, 0, null, null, null, WasRemovalPrevented: false, null, null, position, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, pileType, null, null, null, 0m, card, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingOrbPassiveTriggerCountPrefix(OrbModel orb, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingOrbPassiveTriggerCount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingOrbPassiveTriggerCount, 0, orb, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingOrbPassiveTriggerCountPostfix(OrbModel orb, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingOrbPassiveTriggerCount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingOrbPassiveTriggerCount, 0, orb, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingCardRewardOptionsPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingCardRewardOptions))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingCardRewardOptions, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingCardRewardOptionsPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingCardRewardOptions))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingCardRewardOptions, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingDamageAmountPrefix(CardModel? cardSource, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingDamageAmount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingDamageAmount, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingDamageAmountPostfix(CardModel? cardSource, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingDamageAmount))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingDamageAmount, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingEnergyGainPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingEnergyGain))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingEnergyGain, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingEnergyGainPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingEnergyGain))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingEnergyGain, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingHandDrawPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingHandDraw))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingHandDraw, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingHandDrawPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingHandDraw))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingHandDraw, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPreventingDrawPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPreventingDraw))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPreventingDraw, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPreventingDrawPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPreventingDraw))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPreventingDraw, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingHpLostBeforeOstyPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingHpLostBeforeOsty))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingHpLostBeforeOsty, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingHpLostBeforeOstyPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingHpLostBeforeOsty))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingHpLostBeforeOsty, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingHpLostAfterOstyPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingHpLostAfterOsty))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingHpLostAfterOsty, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingHpLostAfterOstyPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingHpLostAfterOsty))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingHpLostAfterOsty, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingPowerAmountReceivedPrefix(PowerModel power, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingPowerAmountReceived))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingPowerAmountReceived, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingPowerAmountReceivedPostfix(PowerModel power, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingPowerAmountReceived))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingPowerAmountReceived, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingPowerAmountGivenPrefix(PowerModel power, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingPowerAmountGiven))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingPowerAmountGiven, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingPowerAmountGivenPostfix(PowerModel power, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingPowerAmountGiven))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingPowerAmountGiven, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingRewardsPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingRewards))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingRewards, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingRewardsPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingRewards))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingRewards, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterOrbChanneledPrefix(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterOrbChanneled))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterOrbChanneled, 0, orb, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterOrbChanneledPostfix(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterOrbChanneled))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterOrbChanneled, 0, orb, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterOrbEvokedPrefix(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterOrbEvoked))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterOrbEvoked, 0, orb, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, targets, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterOrbEvokedPostfix(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterOrbEvoked))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterOrbEvoked, 0, orb, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, targets, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterOstyRevivedPrefix(Creature osty, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterOstyRevived))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterOstyRevived, 0, null, osty, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterOstyRevivedPostfix(Creature osty, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterOstyRevived))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterOstyRevived, 0, null, osty, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforePotionUsedPrefix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforePotionUsed))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforePotionUsed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforePotionUsedPostfix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforePotionUsed))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforePotionUsed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPotionUsedPrefix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPotionUsed))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPotionUsed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPotionUsedPostfix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPotionUsed))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPotionUsed, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, null, target);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPotionDiscardedPrefix(PotionModel potion, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPotionDiscarded))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPotionDiscarded, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPotionDiscardedPostfix(PotionModel potion, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPotionDiscarded))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPotionDiscarded, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPotionProcuredPrefix(PotionModel potion, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPotionProcured))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPotionProcured, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPotionProcuredPostfix(PotionModel potion, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPotionProcured))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPotionProcured, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, potion, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforePowerAmountChangedPrefix(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforePowerAmountChanged))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforePowerAmountChanged, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, target, applier);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforePowerAmountChangedPostfix(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforePowerAmountChanged))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforePowerAmountChanged, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, target, applier);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPowerAmountChangedPrefix(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPowerAmountChanged))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPowerAmountChanged, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, null, applier);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPowerAmountChangedPostfix(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPowerAmountChanged))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPowerAmountChanged, 0, null, null, null, WasRemovalPrevented: false, null, power, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, cardSource, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, null, applier);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPreventingBlockClearPrefix(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPreventingBlockClear))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPreventingBlockClear, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, preventer, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPreventingBlockClearPostfix(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPreventingBlockClear))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPreventingBlockClear, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, preventer, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPreventingDeathPrefix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPreventingDeath))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPreventingDeath, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterPreventingDeathPostfix(Creature creature, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterPreventingDeath))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterPreventingDeath, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, creature);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRestSiteHealPrefix(Player player, bool isMimicked, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRestSiteHeal))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRestSiteHeal, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, isMimicked, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRestSiteHealPostfix(Player player, bool isMimicked, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRestSiteHeal))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRestSiteHeal, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, isMimicked, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRestSiteSmithPrefix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRestSiteSmith))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRestSiteSmith, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRestSiteSmithPostfix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRestSiteSmith))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRestSiteSmith, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRewardTakenPrefix(Player player, Reward reward, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRewardTaken))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRewardTaken, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, reward);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRewardTakenPostfix(Player player, Reward reward, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRewardTaken))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRewardTaken, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, reward);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeRoomEnteredPrefix(AbstractRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeRoomEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeRoomEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, room);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeRoomEnteredPostfix(AbstractRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.BeforeRoomEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeRoomEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, room);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRoomEnteredPrefix(AbstractRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRoomEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRoomEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, room);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterRoomEnteredPostfix(AbstractRoom room, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterRoomEntered))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterRoomEntered, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, room);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterShufflePrefix(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterShuffle))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterShuffle, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, shuffler, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterShufflePostfix(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterShuffle))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterShuffle, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, shuffler, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterStarsSpentPrefix(int amount, Player spender, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterStarsSpent))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterStarsSpent, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, spender);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterStarsSpentPostfix(int amount, Player spender, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterStarsSpent))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterStarsSpent, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, spender);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterStarsGainedPrefix(int amount, Player gainer, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterStarsGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterStarsGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, gainer);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterStarsGainedPostfix(int amount, Player gainer, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterStarsGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterStarsGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, gainer);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterForgePrefix(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterForge))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterForge, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, forger, null, null, null, amount, null, null, null, null, null, null, source);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterForgePostfix(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterForge))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterForge, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, forger, null, null, null, amount, null, null, null, null, null, null, source);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSummonPrefix(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterSummon))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSummon, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, null, null, null, summoner);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSummonPostfix(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterSummon))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSummon, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount, null, null, null, null, null, null, null, summoner);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterTakingExtraTurnPrefix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTakingExtraTurn))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTakingExtraTurn, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterTakingExtraTurnPostfix(Player player, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTakingExtraTurn))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTakingExtraTurn, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterTargetingBlockedVfxPrefix(Creature blocker, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTargetingBlockedVfx))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTargetingBlockedVfx, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, blocker, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterTargetingBlockedVfxPostfix(Creature blocker, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTargetingBlockedVfx))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTargetingBlockedVfx, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, blocker, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStartPrefix(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task BeforeSideTurnStartPostfix(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.BeforeSideTurnStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.BeforeSideTurnStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, choiceContext, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnStartPrefix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnStartPostfix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnStart))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnStart, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnStartLatePrefix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnStartLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnStartLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterSideTurnStartLatePostfix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (Enumerable.Contains(Timings, Timing.AfterSideTurnStartLate))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterSideTurnStartLate, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, side, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, 0m, null, null, null, combatState, null, null, null, null, 0, null, null, null, participants);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingGoldGainedPrefix(Player player, decimal amount, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingGoldGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingGoldGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount);
				return OnTimingPrefix(context);
			}
			return Task.CompletedTask;
		}

		public override Task AfterModifyingGoldGainedPostfix(Player player, decimal amount, ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterModifyingGoldGained))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterModifyingGoldGained, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, player, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0, 0m, null, CausedByEthereal: false, 0f, null, null, null, null, null, amount);
				return OnTimingPostfix(context);
			}
			return Task.CompletedTask;
		}

		public override void AfterTransformedFromPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTransformedFrom))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTransformedFrom, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				OnTimingPrefix(context);
			}
		}

		public override void AfterTransformedFromPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTransformedFrom))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTransformedFrom, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				OnTimingPostfix(context);
			}
		}

		public override void AfterTransformedToPrefix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTransformedTo))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTransformedTo, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				OnTimingPrefix(context);
			}
		}

		public override void AfterTransformedToPostfix(ComponentContext componentContext)
		{
			if (Enumerable.Contains(Timings, Timing.AfterTransformedTo))
			{
				OnTimingContext context = new OnTimingContext(Timing.AfterTransformedTo, 0, null, null, null, WasRemovalPrevented: false, null, null, (CardPilePosition)0, (AutoPlayType)0, null, null, null, null, null, null, null, (PileType)0, FromHandDraw: false, null, (PileType)0, null, null, null, 0m, null, (CombatSide)0, IsMimicked: false, null, (ValueProp)0);
				OnTimingPostfix(context);
			}
		}

		public override void Serialize(ArrayBufferWriter<byte> writer)
		{
			base.Serialize(writer);
			if (Timings == null)
			{
				SerializationUtils.WriteBoolean(writer, value: false);
				return;
			}
			SerializationUtils.WriteBoolean(writer, value: true);
			int num = Timings.Length;
			SerializationUtils.WriteCount(writer, num);
			for (int i = 0; i < num; i++)
			{
				SerializationUtils.WriteInt32(writer, (int)Timings[i]);
			}
		}

		public override bool Deserialize(ref ReadOnlySpan<byte> reader)
		{
			if (!base.Deserialize(ref reader))
			{
				return false;
			}
			if (!SerializationUtils.TryReadBoolean(ref reader, out var value))
			{
				return false;
			}
			if (!value)
			{
				return false;
			}
			if (!SerializationUtils.TryReadCount(ref reader, out var count))
			{
				return false;
			}
			Timing[] array = new Timing[count];
			for (int i = 0; i < count; i++)
			{
				Timing timing = Timing.OnPlay;
				if (!SerializationUtils.TryReadInt32(ref reader, out var value2))
				{
					return false;
				}
				timing = (Timing)value2;
				array[i] = timing;
			}
			Timings = array;
			return true;
		}
	}
	public record OnTimingContext(Timing Timing, int ActIndex = 0, OrbModel Orb = null, Creature Osty = null, ActMap Map = null, bool WasRemovalPrevented = false, MerchantEntry ItemPurchased = null, PowerModel Power = null, CardPilePosition Position = (CardPilePosition)0, AutoPlayType Type = (AutoPlayType)0, IEnumerable<Creature> Targets = null, Player Shuffler = null, IReadOnlyList<Reward> Rewards = null, PlayerChoiceContext Context = null, PlayerChoiceContext ChoiceContext = null, AbstractModel Preventer = null, Player Player = null, PileType OldPileType = (PileType)0, bool FromHandDraw = false, CardModel? CardSource = null, PileType PileType = (PileType)0, PotionModel Potion = null, Creature Blocker = null, Creature? Dealer = null, decimal Delta = 0m, CardModel Card = null, CombatSide Side = (CombatSide)0, bool IsMimicked = false, Player? Creator = null, ValueProp Props = (ValueProp)0, decimal ModifiedAmount = 0m, DamageResult Result = null, bool CausedByEthereal = false, float DeathAnimLength = 0f, CardPlay? CardPlay = null, Player Forger = null, Creature Creature = null, AbstractRoom Room = null, AttackCommand Command = null, decimal Amount = 0m, Reward Reward = null, Player Spender = null, Player Gainer = null, ICombatState CombatState = null, Creature? Target = null, Creature? Applier = null, AbstractModel? Source = null, Player Summoner = null, int GoldSpent = 0, IReadOnlyList<Creature> Creatures = null, IReadOnlyCollection<CardModel> FlushedCards = null, IReadOnlyCollection<CardModel> RetainedCards = null, IEnumerable<Creature> Participants = null)
	{
		[CompilerGenerated]
		public void Deconstruct(out Timing Timing, out int ActIndex, out OrbModel Orb, out Creature Osty, out ActMap Map, out bool WasRemovalPrevented, out MerchantEntry ItemPurchased, out PowerModel Power, out CardPilePosition Position, out AutoPlayType Type, out IEnumerable<Creature> Targets, out Player Shuffler, out IReadOnlyList<Reward> Rewards, out PlayerChoiceContext Context, out PlayerChoiceContext ChoiceContext, out AbstractModel Preventer, out Player Player, out PileType OldPileType, out bool FromHandDraw, out CardModel? CardSource, out PileType PileType, out PotionModel Potion, out Creature Blocker, out Creature? Dealer, out decimal Delta, out CardModel Card, out CombatSide Side, out bool IsMimicked, out Player? Creator, out ValueProp Props, out decimal ModifiedAmount, out DamageResult Result, out bool CausedByEthereal, out float DeathAnimLength, out CardPlay? CardPlay, out Player Forger, out Creature Creature, out AbstractRoom Room, out AttackCommand Command, out decimal Amount, out Reward Reward, out Player Spender, out Player Gainer, out ICombatState CombatState, out Creature? Target, out Creature? Applier, out AbstractModel? Source, out Player Summoner, out int GoldSpent, out IReadOnlyList<Creature> Creatures, out IReadOnlyCollection<CardModel> FlushedCards, out IReadOnlyCollection<CardModel> RetainedCards, out IEnumerable<Creature> Participants)
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Expected I4, but got Unknown
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected I4, but got Unknown
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009f: Expected I4, but got Unknown
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Expected I4, but got Unknown
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f4: Expected I4, but got Unknown
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010f: Expected I4, but got Unknown
			Timing = this.Timing;
			ActIndex = this.ActIndex;
			Orb = this.Orb;
			Osty = this.Osty;
			Map = this.Map;
			WasRemovalPrevented = this.WasRemovalPrevented;
			ItemPurchased = this.ItemPurchased;
			Power = this.Power;
			Position = (CardPilePosition)(int)this.Position;
			Type = (AutoPlayType)(int)this.Type;
			Targets = this.Targets;
			Shuffler = this.Shuffler;
			Rewards = this.Rewards;
			Context = this.Context;
			ChoiceContext = this.ChoiceContext;
			Preventer = this.Preventer;
			Player = this.Player;
			OldPileType = (PileType)(int)this.OldPileType;
			FromHandDraw = this.FromHandDraw;
			CardSource = this.CardSource;
			PileType = (PileType)(int)this.PileType;
			Potion = this.Potion;
			Blocker = this.Blocker;
			Dealer = this.Dealer;
			Delta = this.Delta;
			Card = this.Card;
			Side = (CombatSide)(int)this.Side;
			IsMimicked = this.IsMimicked;
			Creator = this.Creator;
			Props = (ValueProp)(int)this.Props;
			ModifiedAmount = this.ModifiedAmount;
			Result = this.Result;
			CausedByEthereal = this.CausedByEthereal;
			DeathAnimLength = this.DeathAnimLength;
			CardPlay = this.CardPlay;
			Forger = this.Forger;
			Creature = this.Creature;
			Room = this.Room;
			Command = this.Command;
			Amount = this.Amount;
			Reward = this.Reward;
			Spender = this.Spender;
			Gainer = this.Gainer;
			CombatState = this.CombatState;
			Target = this.Target;
			Applier = this.Applier;
			Source = this.Source;
			Summoner = this.Summoner;
			GoldSpent = this.GoldSpent;
			Creatures = this.Creatures;
			FlushedCards = this.FlushedCards;
			RetainedCards = this.RetainedCards;
			Participants = this.Participants;
		}
	}
}
namespace MinionLib.Component.Patches
{
	public static class CardComponentDyanamicVarsUpdatePatch
	{
		[HarmonyPatch(typeof(CardModel), "UpdateDynamicVarPreview")]
		[HarmonyPostfix]
		private static void UpdateDynamicVarPreviewPostfix(CardModel __instance, object previewMode, Creature? target, object dynamicVarSet)
		{
			if (!(__instance is IComponentsCardModel componentsCardModel))
			{
				return;
			}
			bool flag = __instance.CombatState != null;
			foreach (ICardComponent component in componentsCardModel.Components)
			{
				foreach (DynamicVar value in component.DynamicVars.Values)
				{
					value.UpdateCardPreview(__instance, (dynamic)previewMode, target, flag);
				}
			}
		}

		[HarmonyPatch(typeof(CardModel), "FinalizeUpgradeInternal")]
		[HarmonyPostfix]
		private static void FinalizeUpgradeInternalPostfix(CardModel __instance)
		{
			if (!(__instance is IComponentsCardModel componentsCardModel))
			{
				return;
			}
			foreach (DynamicVarSet item in componentsCardModel.Components.Select((ICardComponent c) => c.DynamicVars))
			{
				item.FinalizeUpgrade();
			}
		}
	}
	[HarmonyPatch]
	public static class ComponentDescriptionRawCachePatch
	{
		public const string CardsTable = "cards";

		public const string PrefixToken = "{CompPre}";

		public const string PostfixToken = "{CompPost}";

		[HarmonyPatch(/*Could not decode attribute arguments.*/)]
		[HarmonyPostfix]
		private static void DescriptionGetterPostfix(CardModel __instance, LocString __result)
		{
			if (__instance is IComponentsCardModel)
			{
				string locEntryKey = __result.LocEntryKey;
				if (!string.IsNullOrWhiteSpace(locEntryKey) && !ComponentDescriptionRawCache.Contains(locEntryKey))
				{
					string rawText = (__result.Exists() ? __result.GetRawText() : "");
					ComponentDescriptionRawCache.Set(locEntryKey, InjectCompTokens(rawText));
				}
			}
		}

		[HarmonyPatch(typeof(LocString), "GetRawText")]
		[HarmonyPrefix]
		private static bool GetRawTextPrefix(LocString __instance, ref string __result)
		{
			if (!string.Equals(__instance.LocTable, "cards", StringComparison.Ordinal))
			{
				return true;
			}
			if (!ComponentDescriptionRawCache.TryGet(__instance.LocEntryKey, out string rawText))
			{
				return true;
			}
			__result = rawText;
			return false;
		}

		[HarmonyPatch(typeof(LocManager), "SetLanguage")]
		[HarmonyPostfix]
		private static void SetLanguagePostfix()
		{
			ComponentDescriptionRawCache.Clear();
		}

		private static string InjectCompTokens(string rawText)
		{
			string text = rawText ?? "";
			if (!text.Contains("{CompPre}", StringComparison.Ordinal))
			{
				text = (string.IsNullOrWhiteSpace(text) ? "{CompPre}" : ("{CompPre}" + text));
			}
			if (!text.Contains("{CompPost}", StringComparison.Ordinal))
			{
				text = (string.IsNullOrWhiteSpace(text) ? "{CompPost}" : (text + "{CompPost}"));
			}
			return text;
		}
	}
	[HarmonyPatch(typeof(CardModel), "FromSerializable")]
	public static class FrickYanoPatch
	{
		private const string BlobPropertyName = "MinionLibComponentStateBlob";

		[HarmonyPostfix]
		[HarmonyPriority(0)]
		public static void RestoreSavedComponentState(SerializableCard save, CardModel __result)
		{
			if (__result is ComponentsCardModel componentsCardModel)
			{
				int[] array = save.Props?.intArrays?.Where(delegate(SavedProperty<int[]> prop)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					return prop.name == "MinionLibComponentStateBlob";
				}).Select(delegate(SavedProperty<int[]> prop)
				{
					//IL_0000: Unknown result type (might be due to invalid IL or missing references)
					return prop.value;
				}).FirstOrDefault();
				if (array != null)
				{
					componentsCardModel.MinionLibComponentStateBlob = array.ToArray();
					componentsCardModel.EnsureComponentsInitialized();
				}
			}
		}
	}
	[HarmonyPatch(typeof(NetFullCombatState), "ToString")]
	public class NetFullCombatStateComponentsLogPatch
	{
		public static void AppendComponentInfo(StringBuilder sb, CardState card)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			sb.Append(card.card.GetComponentsLogString(2, "\t"));
		}

		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Expected O, but got Unknown
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Expected O, but got Unknown
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Expected O, but got Unknown
			//IL_0134: Unknown result type (might be due to invalid IL or missing references)
			//IL_013a: Expected O, but got Unknown
			//IL_0176: Unknown result type (might be due to invalid IL or missing references)
			//IL_0180: Expected O, but got Unknown
			//IL_0183: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Expected O, but got Unknown
			//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b3: Expected O, but got Unknown
			CodeMatcher val = new CodeMatcher(instructions, il);
			val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((OpCode?)OpCodes.Call, (object)AccessTools.PropertyGetter(typeof(List<CardState>.Enumerator), "Current"), (string)null)
			});
			if (!val.IsValid)
			{
				throw new Exception("Transpiler 失败: 找不到 Enumerator.Current");
			}
			val.Advance(1);
			object operand = val.Operand;
			OpCode opCode = ((val.Opcode == OpCodes.Stloc_S) ? OpCodes.Ldloc_S : OpCodes.Ldloc);
			val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((OpCode?)OpCodes.Call, (object)AccessTools.Method(typeof(List<CardState>.Enumerator), "MoveNext", (Type[])null, (Type[])null), (string)null)
			});
			if (!val.IsValid)
			{
				throw new Exception("Transpiler 失败: 找不到 Enumerator.MoveNext");
			}
			val.Advance(-1);
			int pos = val.Pos;
			CodeInstruction instruction = val.Instruction;
			List<Label> extractedLabels = new List<Label>(instruction.labels);
			instruction.labels.Clear();
			Label label = il.DefineLabel();
			instruction.labels.Add(label);
			val.MatchStartBackwards((CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((Func<CodeInstruction, bool>)((CodeInstruction i) => (i.opcode == OpCodes.Br || i.opcode == OpCodes.Br_S) && i.operand is Label item && extractedLabels.Contains(item)), (string)null)
			});
			if (val.IsValid)
			{
				val.Instruction.operand = label;
			}
			val.Advance(pos - val.Pos);
			List<CodeInstruction> list = new List<CodeInstruction>
			{
				new CodeInstruction(OpCodes.Ldloc_0, (object)null),
				new CodeInstruction(opCode, operand),
				new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(NetFullCombatStateComponentsLogPatch), "AppendComponentInfo", (Type[])null, (Type[])null))
			};
			list[0].labels.AddRange(extractedLabels);
			val.InsertAndAdvance((IEnumerable<CodeInstruction>)list);
			return val.InstructionEnumeration();
		}
	}
}
namespace MinionLib.Component.Interfaces
{
	public interface ICardComponent : IGeneratedBinarySerializable
	{
		string ComponentId { get; }

		IComponentsCardModel? ComponentsCard { get; }

		CardModel? Card
		{
			get
			{
				IComponentsCardModel? componentsCard = ComponentsCard;
				return (CardModel?)((componentsCard is CardModel) ? componentsCard : null);
			}
		}

		DynamicVarSet DynamicVars { get; }

		bool ShouldGlowGoldInternal => false;

		bool ShouldGlowRedInternal => false;

		Color? GlowColor => null;

		TargetType? ExtraTargetType => null;

		CardType? CardTypeOverride => null;

		CardRarity? CardRarityOverride => null;

		IEnumerable<CardTag> ExtraTags => Array.Empty<CardTag>();

		bool IsPlayable => true;

		bool HasTurnEndInHandEffect => false;

		IEnumerable<IHoverTip> HoverTips => Array.Empty<IHoverTip>();

		void Attach(IComponentsCardModel card, bool isInternal = false);

		void Detach(bool isInternal = false);

		ICardComponent DeepClone();

		bool TryMergeWith(ICardComponent incoming, ApplyComponentOptions options, out ICardComponent? merged);

		bool TrySubtractiveMergeWith(ICardComponent incoming, ApplyComponentOptions options, out ICardComponent? merged);

		PileType? GetResultPileTypeForCardPlay()
		{
			return null;
		}

		string GetFormattedPrefix(Dictionary<string, object> argsFromCard);

		string GetFormattedPostfix(Dictionary<string, object> argsFromCard);

		bool CanHandleRightClickLocal(RightClickContext context)
		{
			return CanHandleRightClick(context);
		}

		bool CanHandleRightClick(RightClickContext context)
		{
			return false;
		}

		Task OnRightClick(PlayerChoiceContext choiceContext, RightClickContext clickContext)
		{
			return Task.CompletedTask;
		}

		void OnUpgrade(ComponentContext componentContext)
		{
		}

		void AfterDowngraded(ComponentContext componentContext)
		{
		}

		Task OnPlayPrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task OnPlayPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task OnEnqueuePlayVfxPrefix(Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task OnEnqueuePlayVfxPostfix(Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task OnTurnEndInHandPrefix(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task OnTurnEndInHandPostfix(PlayerChoiceContext choiceContext, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCardPlayedPrefix(CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCardPlayedPostfix(CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardPlayedPrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardPlayedPostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardPlayedLatePrefix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardPlayedLatePostfix(PlayerChoiceContext choiceContext, CardPlay cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPlayerTurnStartEarlyPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPlayerTurnStartEarlyPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPlayerTurnStartPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPlayerTurnStartPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPlayerTurnStartLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPlayerTurnStartLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPostPlayPhaseEnteredPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPostPlayPhaseEnteredPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPrePlayPhaseEnteredEarlyPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPrePlayPhaseEnteredEarlyPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPrePlayPhaseEnteredPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPrePlayPhaseEnteredPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPrePlayPhaseEnteredLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAutoPrePlayPhaseEnteredLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnEndVeryEarlyPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnEndVeryEarlyPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnEndEarlyPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnEndEarlyPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnEndPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnEndPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnEndPrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnEndPostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnEndLatePrefix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnEndLatePostfix(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterActEnteredPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterActEnteredPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAddToDeckPreventedPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAddToDeckPreventedPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeAttackPrefix(AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeAttackPostfix(AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAttackPrefix(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterAttackPostfix(PlayerChoiceContext choiceContext, AttackCommand command, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterBlockClearedPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterBlockClearedPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeBlockGainedPrefix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeBlockGainedPostfix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterBlockGainedPrefix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterBlockGainedPostfix(Creature creature, decimal amount, ValueProp props, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterBlockBrokenPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterBlockBrokenPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardChangedPilesPrefix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardChangedPilesPostfix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardChangedPilesLatePrefix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardChangedPilesLatePostfix(CardModel card, PileType oldPileType, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardDiscardedPrefix(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardDiscardedPostfix(PlayerChoiceContext choiceContext, CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardDrawnEarlyPrefix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardDrawnEarlyPostfix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardDrawnPrefix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardDrawnPostfix(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardEnteredCombatPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardEnteredCombatPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardGeneratedForCombatPrefix(CardModel card, Player? creator, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardGeneratedForCombatPostfix(CardModel card, Player? creator, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardExhaustedPrefix(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCardExhaustedPostfix(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCardAutoPlayedPrefix(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCardAutoPlayedPostfix(CardModel card, Creature? target, AutoPlayType type, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCombatStartPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCombatStartPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCombatStartLatePrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCombatStartLatePostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCombatEndPrefix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCombatEndPostfix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCombatVictoryEarlyPrefix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCombatVictoryEarlyPostfix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCombatVictoryPrefix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCombatVictoryPostfix(CombatRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCreatureAddedToCombatPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCreatureAddedToCombatPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCurrentHpChangedPrefix(Creature creature, decimal delta, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterCurrentHpChangedPostfix(Creature creature, decimal delta, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDamageGivenPrefix(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDamageGivenPostfix(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeDamageReceivedPrefix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeDamageReceivedPostfix(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDamageReceivedPrefix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDamageReceivedPostfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDamageReceivedLatePrefix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDamageReceivedLatePostfix(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeDeathPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeDeathPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDeathPrefix(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDeathPostfix(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDiedToDoomPrefix(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterDiedToDoomPostfix(PlayerChoiceContext choiceContext, IReadOnlyList<Creature> creatures, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterEnergyResetPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterEnergyResetPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterEnergyResetLatePrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterEnergyResetLatePostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterEnergySpentPrefix(CardModel card, int amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterEnergySpentPostfix(CardModel card, int amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCardRemovedPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeCardRemovedPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeFlushPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeFlushPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeFlushLatePrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeFlushLatePostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterFlushPrefix(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterFlushPostfix(PlayerChoiceContext choiceContext, Player player, IReadOnlyCollection<CardModel> flushedCards, IReadOnlyCollection<CardModel> retainedCards, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterGoldGainedPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterGoldGainedPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeHandDrawPrefix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeHandDrawPostfix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeHandDrawLatePrefix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeHandDrawLatePostfix(Player player, PlayerChoiceContext choiceContext, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterHandEmptiedPrefix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterHandEmptiedPostfix(PlayerChoiceContext choiceContext, Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterItemPurchasedPrefix(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterItemPurchasedPostfix(Player player, MerchantEntry itemPurchased, int goldSpent, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterMapGeneratedPrefix(ActMap map, int actIndex, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterMapGeneratedPostfix(ActMap map, int actIndex, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingBlockAmountPrefix(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingBlockAmountPostfix(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingCardPlayCountPrefix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingCardPlayCountPostfix(CardModel card, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingCardPlayResultPileOrPositionPrefix(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingCardPlayResultPileOrPositionPostfix(CardModel card, PileType pileType, CardPilePosition position, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingOrbPassiveTriggerCountPrefix(OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingOrbPassiveTriggerCountPostfix(OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingCardRewardOptionsPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingCardRewardOptionsPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingDamageAmountPrefix(CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingDamageAmountPostfix(CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingEnergyGainPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingEnergyGainPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingHandDrawPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingHandDrawPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPreventingDrawPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPreventingDrawPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingHpLostBeforeOstyPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingHpLostBeforeOstyPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingHpLostAfterOstyPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingHpLostAfterOstyPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingPowerAmountReceivedPrefix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingPowerAmountReceivedPostfix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingPowerAmountGivenPrefix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingPowerAmountGivenPostfix(PowerModel power, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingRewardsPrefix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingRewardsPostfix(ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterOrbChanneledPrefix(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterOrbChanneledPostfix(PlayerChoiceContext choiceContext, Player player, OrbModel orb, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterOrbEvokedPrefix(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterOrbEvokedPostfix(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterOstyRevivedPrefix(Creature osty, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterOstyRevivedPostfix(Creature osty, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforePotionUsedPrefix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforePotionUsedPostfix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPotionUsedPrefix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPotionUsedPostfix(PotionModel potion, Creature? target, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPotionDiscardedPrefix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPotionDiscardedPostfix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPotionProcuredPrefix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPotionProcuredPostfix(PotionModel potion, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforePowerAmountChangedPrefix(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforePowerAmountChangedPostfix(PowerModel power, decimal amount, Creature target, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPowerAmountChangedPrefix(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPowerAmountChangedPostfix(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPreventingBlockClearPrefix(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPreventingBlockClearPostfix(AbstractModel preventer, Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPreventingDeathPrefix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterPreventingDeathPostfix(Creature creature, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRestSiteHealPrefix(Player player, bool isMimicked, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRestSiteHealPostfix(Player player, bool isMimicked, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRestSiteSmithPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRestSiteSmithPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRewardTakenPrefix(Player player, Reward reward, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRewardTakenPostfix(Player player, Reward reward, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeRoomEnteredPrefix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeRoomEnteredPostfix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRoomEnteredPrefix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterRoomEnteredPostfix(AbstractRoom room, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterShufflePrefix(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterShufflePostfix(PlayerChoiceContext choiceContext, Player shuffler, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterStarsSpentPrefix(int amount, Player spender, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterStarsSpentPostfix(int amount, Player spender, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterStarsGainedPrefix(int amount, Player gainer, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterStarsGainedPostfix(int amount, Player gainer, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterForgePrefix(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterForgePostfix(decimal amount, Player forger, AbstractModel? source, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSummonPrefix(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSummonPostfix(PlayerChoiceContext choiceContext, Player summoner, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterTakingExtraTurnPrefix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterTakingExtraTurnPostfix(Player player, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterTargetingBlockedVfxPrefix(Creature blocker, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterTargetingBlockedVfxPostfix(Creature blocker, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnStartPrefix(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task BeforeSideTurnStartPostfix(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnStartPrefix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnStartPostfix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnStartLatePrefix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterSideTurnStartLatePostfix(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingGoldGainedPrefix(Player player, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		Task AfterModifyingGoldGainedPostfix(Player player, decimal amount, ComponentContext componentContext)
		{
			return Task.CompletedTask;
		}

		void AfterTransformedFromPrefix(ComponentContext componentContext)
		{
		}

		void AfterTransformedFromPostfix(ComponentContext componentContext)
		{
		}

		void AfterTransformedToPrefix(ComponentContext componentContext)
		{
		}

		void AfterTransformedToPostfix(ComponentContext componentContext)
		{
		}

		string IGeneratedBinarySerializable.ToLogString(int depth, string indentChars)
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = string.Concat(Enumerable.Repeat(indentChars, depth));
			PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length != 0 || !propertyInfo.IsDefined(typeof(ComponentStateAttribute), inherit: true))
				{
					continue;
				}
				object value2 = propertyInfo.GetValue(this);
				string name = propertyInfo.Name;
				if (value2 != null)
				{
					if (!(value2 is IGeneratedBinarySerializable generatedBinarySerializable))
					{
						if (value2 is IEnumerable enumerable && !(enumerable is string))
						{
							StringBuilder stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder3 = stringBuilder2;
							StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
							handler.AppendFormatted(value);
							handler.AppendFormatted(name);
							handler.AppendLiteral(": ");
							stringBuilder3.AppendLine(ref handler);
							foreach (object item in enumerable)
							{
								if (item != null)
								{
									if (item is IGeneratedBinarySerializable generatedBinarySerializable2)
									{
										stringBuilder2 = stringBuilder;
										StringBuilder stringBuilder4 = stringBuilder2;
										handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
										handler.AppendFormatted(value);
										handler.AppendFormatted(indentChars);
										handler.AppendLiteral("- ");
										stringBuilder4.AppendLine(ref handler);
										stringBuilder.Append(generatedBinarySerializable2.ToLogString(depth + 2, indentChars));
									}
									else
									{
										stringBuilder2 = stringBuilder;
										StringBuilder stringBuilder5 = stringBuilder2;
										handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
										handler.AppendFormatted(value);
										handler.AppendFormatted(indentChars);
										handler.AppendLiteral("- ");
										handler.AppendFormatted<object>(item);
										stringBuilder5.AppendLine(ref handler);
									}
								}
								else
								{
									stringBuilder2 = stringBuilder;
									StringBuilder stringBuilder6 = stringBuilder2;
									handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
									handler.AppendFormatted(value);
									handler.AppendFormatted(indentChars);
									handler.AppendLiteral("- null");
									stringBuilder6.AppendLine(ref handler);
								}
							}
						}
						else
						{
							StringBuilder stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder7 = stringBuilder2;
							StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
							handler.AppendFormatted(value);
							handler.AppendFormatted(name);
							handler.AppendLiteral(": ");
							handler.AppendFormatted<object>(value2);
							stringBuilder7.AppendLine(ref handler);
						}
					}
					else
					{
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder8 = stringBuilder2;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 2, stringBuilder2);
						handler.AppendFormatted(value);
						handler.AppendFormatted(name);
						handler.AppendLiteral(":");
						stringBuilder8.AppendLine(ref handler);
						stringBuilder.Append(generatedBinarySerializable.ToLogString(depth + 1, indentChars));
					}
				}
				else
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder9 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
					handler.AppendFormatted(value);
					handler.AppendFormatted(name);
					handler.AppendLiteral(": null");
					stringBuilder9.AppendLine(ref handler);
				}
			}
			return stringBuilder.ToString();
		}

		int ModifyAttackHitCount(AttackCommand attack, int hitCount)
		{
			return hitCount;
		}

		decimal ModifyBlockAdditive(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			return 0m;
		}

		decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
		{
			return 1m;
		}

		int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
		{
			return playCount;
		}

		(PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			return (pileType, position);
		}

		int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
		{
			return triggerCount;
		}

		CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
		{
			return options;
		}

		CardCreationOptions ModifyCardRewardCreationOptionsLate(Player player, CardCreationOptions options)
		{
			return options;
		}

		decimal ModifyCardRewardUpgradeOdds(Player player, CardModel card, decimal odds)
		{
			return odds;
		}

		decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return 0m;
		}

		decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return decimal.MaxValue;
		}

		decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return 1m;
		}

		decimal ModifyEnergyGain(Player player, decimal amount)
		{
			return amount;
		}

		decimal ModifyGoldGained(Player player, decimal amount)
		{
			return amount;
		}

		ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
		{
			return map;
		}

		ActMap ModifyGeneratedMapLate(IRunState runState, ActMap map, int actIndex)
		{
			return map;
		}

		decimal ModifyHandDraw(Player player, decimal count)
		{
			return count;
		}

		decimal ModifyHandDrawLate(Player player, decimal count)
		{
			return count;
		}

		decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		decimal ModifyHpLostBeforeOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		decimal ModifyHpLostAfterOstyLate(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
		{
			return amount;
		}

		decimal ModifyMaxEnergy(Player player, decimal amount)
		{
			return amount;
		}

		IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
		{
			return options;
		}

		CardRarity ModifyMerchantCardRarity(Player player, CardRarity rarity)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return rarity;
		}

		void ModifyMerchantCardCreationResults(Player player, List<CardCreationResult> cards)
		{
		}

		decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost)
		{
			return cost;
		}

		decimal ModifyOrbValue(OrbModel orb, decimal value)
		{
			return value;
		}

		decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			return 0m;
		}

		decimal ModifyPowerAmountGivenMultiplicative(PowerModel power, Creature giver, decimal amount, Creature? target, CardModel? cardSource)
		{
			return 1m;
		}

		decimal ModifyRestSiteHealAmount(Creature creature, decimal amount)
		{
			return amount;
		}

		void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
		{
		}

		decimal ModifySummonAmount(Player summoner, decimal amount, AbstractModel? source)
		{
			return amount;
		}

		Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
		{
			return target;
		}

		EventModel ModifyNextEvent(EventModel currentEvent)
		{
			return currentEvent;
		}

		IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
		{
			return roomTypes;
		}

		float ModifyOddsIncreaseForUnrolledRoomType(RoomType roomType, float oddsIncrease)
		{
			return oddsIncrease;
		}

		int ModifyXValue(CardModel card, int originalValue)
		{
			return originalValue;
		}

		bool TryModifyCardBeingAddedToDeck(CardModel card, out CardModel? newCard)
		{
			newCard = null;
			return false;
		}

		bool TryModifyCardBeingAddedToDeckLate(CardModel card, out CardModel? newCard)
		{
			newCard = null;
			return false;
		}

		bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
		{
			return false;
		}

		bool TryModifyCardRewardOptions(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			return false;
		}

		bool TryModifyCardRewardOptionsLate(Player player, List<CardCreationResult> cardRewardOptions, CardCreationOptions creationOptions)
		{
			return false;
		}

		bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			return false;
		}

		bool TryModifyStarCost(CardModel card, decimal originalCost, out decimal modifiedCost)
		{
			modifiedCost = originalCost;
			return false;
		}

		bool TryModifyPowerAmountReceived(PowerModel canonicalPower, Creature target, decimal amount, Creature? applier, out decimal modifiedAmount)
		{
			modifiedAmount = amount;
			return false;
		}

		bool TryModifyRestSiteOptions(Player player, ICollection<RestSiteOption> options)
		{
			return false;
		}

		bool TryModifyRestSiteHealRewards(Player player, List<Reward> rewards, bool isMimicked)
		{
			return false;
		}

		bool TryModifyRewards(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			return false;
		}

		bool TryModifyRewardsLate(Player player, List<Reward> rewards, AbstractRoom? room)
		{
			return false;
		}

		IReadOnlyList<LocString> ModifyExtraRestSiteHealText(Player player, IReadOnlyList<LocString> currentExtraText)
		{
			return currentExtraText;
		}

		bool TryModifyEnergyCostInCombatLate(CardModel card, decimal currentCost, out decimal modifiedCost)
		{
			modifiedCost = currentCost;
			return false;
		}

		bool ShouldAddToDeck(CardModel card)
		{
			return true;
		}

		bool ShouldAfflict(CardModel card, AfflictionModel affliction)
		{
			return true;
		}

		bool ShouldAllowAncient(Player player, AncientEventModel ancient)
		{
			return true;
		}

		bool ShouldAllowHitting(Creature creature)
		{
			return true;
		}

		bool ShouldAllowTargeting(Creature target)
		{
			return true;
		}

		bool ShouldAllowSelectingMoreCardRewards(Player player, CardReward cardReward)
		{
			return false;
		}

		bool ShouldClearBlock(Creature creature)
		{
			return true;
		}

		bool ShouldDie(Creature creature)
		{
			return true;
		}

		bool ShouldDieLate(Creature creature)
		{
			return true;
		}

		bool ShouldDisableRemainingRestSiteOptions(Player player)
		{
			return true;
		}

		bool ShouldDraw(Player player, bool fromHandDraw)
		{
			return true;
		}

		bool ShouldEtherealTrigger(CardModel card)
		{
			return true;
		}

		bool ShouldFlush(Player player)
		{
			return true;
		}

		bool ShouldGainStars(decimal amount, Player player)
		{
			return true;
		}

		bool ShouldGenerateTreasure(Player player)
		{
			return true;
		}

		bool ShouldPayExcessEnergyCostWithStars(Player player)
		{
			return false;
		}

		bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
		{
			return true;
		}

		bool ShouldPlayerResetEnergy(Player player)
		{
			return true;
		}

		bool ShouldProceedToNextMapPoint()
		{
			return true;
		}

		bool ShouldProcurePotion(PotionModel potion, Player player)
		{
			return true;
		}

		bool ShouldPowerBeRemovedOnDeath(PowerModel power)
		{
			return true;
		}

		bool ShouldRefillMerchantEntry(MerchantEntry entry, Player player)
		{
			return false;
		}

		bool ShouldAllowMerchantCardRemoval(Player player)
		{
			return true;
		}

		bool ShouldCreatureBeRemovedFromCombatAfterDeath(Creature creature)
		{
			return true;
		}

		bool ShouldStopCombatFromEnding()
		{
			return false;
		}

		bool ShouldTakeExtraTurn(Player player)
		{
			return false;
		}

		bool ShouldForcePotionReward(Player player, RoomType roomType)
		{
			return false;
		}

		bool ShouldAllowFreeTravel()
		{
			return false;
		}
	}
	public interface IComponentsCardModel
	{
		CardModel AsCardModel
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0007: Expected O, but got Unknown
				return (CardModel)this;
			}
		}

		IReadOnlyList<ICardComponent> Components { get; }

		ICardComponent? AddComponent<T>(T incoming, bool allowMerge = true, bool isUpgrade = false) where T : class, ICardComponent
		{
			return ApplyComponent(incoming, new ApplyComponentOptions(allowMerge, UseSubtractiveMerge: false, isUpgrade));
		}

		ICardComponent? SubtractComponent<T>(T incoming, bool isUpgrade = false) where T : class, ICardComponent
		{
			return ApplyComponent(incoming, new ApplyComponentOptions(AllowMerge: true, UseSubtractiveMerge: true, isUpgrade));
		}

		ICardComponent? ApplyComponent<T>(T incoming, ApplyComponentOptions options) where T : class, ICardComponent;

		ICardComponent? RemoveComponent<T>() where T : class, ICardComponent;

		IReadOnlyList<ICardComponent> RemoveComponents<T>() where T : class, ICardComponent;

		bool RefRemoveComponent(ICardComponent component);

		T? GetComponent<T>() where T : class, ICardComponent;

		IReadOnlyList<T> GetComponents<T>() where T : class, ICardComponent;

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		Task ComponentCallBack(string name, params object?[] args);

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		bool ComponentPredicate(string name, params object?[] args);

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		object? ComponentQuery(string name, params object?[] args);

		[Obsolete("This method is deprecated and should not be called or overridden. Use interface constraints or delegate registry instead.", false)]
		Task<object?> ComponentQueryAsync(string name, params object?[] args);
	}
	public interface IGeneratedBinarySerializable
	{
		void Serialize(ArrayBufferWriter<byte> writer);

		bool Deserialize(ref ReadOnlySpan<byte> reader);

		string ToLogString(int depth = 0, string indentChars = "    ")
		{
			StringBuilder stringBuilder = new StringBuilder();
			string value = string.Concat(Enumerable.Repeat(indentChars, depth));
			PropertyInfo[] properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (!propertyInfo.CanRead || propertyInfo.GetIndexParameters().Length != 0)
				{
					continue;
				}
				object value2 = propertyInfo.GetValue(this);
				string name = propertyInfo.Name;
				if (value2 != null)
				{
					if (!(value2 is IGeneratedBinarySerializable generatedBinarySerializable))
					{
						if (value2 is IEnumerable enumerable && !(enumerable is string))
						{
							StringBuilder stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder3 = stringBuilder2;
							StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
							handler.AppendFormatted(value);
							handler.AppendFormatted(name);
							handler.AppendLiteral(": ");
							stringBuilder3.AppendLine(ref handler);
							foreach (object item in enumerable)
							{
								if (item != null)
								{
									if (item is IGeneratedBinarySerializable generatedBinarySerializable2)
									{
										stringBuilder2 = stringBuilder;
										StringBuilder stringBuilder4 = stringBuilder2;
										handler = new StringBuilder.AppendInterpolatedStringHandler(2, 2, stringBuilder2);
										handler.AppendFormatted(value);
										handler.AppendFormatted(indentChars);
										handler.AppendLiteral("- ");
										stringBuilder4.AppendLine(ref handler);
										stringBuilder.Append(generatedBinarySerializable2.ToLogString(depth + 2, indentChars));
									}
									else
									{
										stringBuilder2 = stringBuilder;
										StringBuilder stringBuilder5 = stringBuilder2;
										handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
										handler.AppendFormatted(value);
										handler.AppendFormatted(indentChars);
										handler.AppendLiteral("- ");
										handler.AppendFormatted<object>(item);
										stringBuilder5.AppendLine(ref handler);
									}
								}
								else
								{
									stringBuilder2 = stringBuilder;
									StringBuilder stringBuilder6 = stringBuilder2;
									handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
									handler.AppendFormatted(value);
									handler.AppendFormatted(indentChars);
									handler.AppendLiteral("- null");
									stringBuilder6.AppendLine(ref handler);
								}
							}
						}
						else
						{
							StringBuilder stringBuilder2 = stringBuilder;
							StringBuilder stringBuilder7 = stringBuilder2;
							StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(2, 3, stringBuilder2);
							handler.AppendFormatted(value);
							handler.AppendFormatted(name);
							handler.AppendLiteral(": ");
							handler.AppendFormatted<object>(value2);
							stringBuilder7.AppendLine(ref handler);
						}
					}
					else
					{
						StringBuilder stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder8 = stringBuilder2;
						StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(1, 2, stringBuilder2);
						handler.AppendFormatted(value);
						handler.AppendFormatted(name);
						handler.AppendLiteral(":");
						stringBuilder8.AppendLine(ref handler);
						stringBuilder.Append(generatedBinarySerializable.ToLogString(depth + 1, indentChars));
					}
				}
				else
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder9 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(6, 2, stringBuilder2);
					handler.AppendFormatted(value);
					handler.AppendFormatted(name);
					handler.AppendLiteral(": null");
					stringBuilder9.AppendLine(ref handler);
				}
			}
			return stringBuilder.ToString();
		}
	}
}
namespace MinionLib.Component.Extensions
{
	public static class ComponentsBlobLogExtension
	{
		extension(SerializableCard save)
		{
			public int[]? MinionLibComponentStateBlob => save.Props?.intArrays?.Where(delegate(SavedProperty<int[]> prop)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return prop.name == "MinionLibComponentStateBlob";
			}).Select(delegate(SavedProperty<int[]> prop)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return prop.value;
			}).FirstOrDefault();

			public IReadOnlyList<ICardComponent>? Components
			{
				get
				{
					int[] array = get_MinionLibComponentStateBlob(save);
					if (array != null)
					{
						return CardComponentStateSerializer.Deserialize(array, null);
					}
					return null;
				}
			}

			public string GetComponentsLogString(int depth = 0, string indentChars = "    ", bool showEmpty = false)
			{
				IReadOnlyList<ICardComponent> readOnlyList = get_Components(save);
				if (readOnlyList == null)
				{
					return "";
				}
				string text = string.Concat(Enumerable.Repeat(indentChars, depth));
				if (readOnlyList.Count == 0)
				{
					if (!showEmpty)
					{
						return "";
					}
					return text + "Components: [Empty]\n";
				}
				return text + "Components: \n" + readOnlyList.ToLogString(depth + 1, indentChars);
			}
		}

		extension(IEnumerable<ICardComponent> components)
		{
			public string ToLogString(int depth = 0, string indentChars = "    ")
			{
				string value = string.Concat(Enumerable.Repeat(indentChars, depth));
				StringBuilder stringBuilder = new StringBuilder();
				foreach (ICardComponent component in components)
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(3, 2, stringBuilder2);
					handler.AppendFormatted(value);
					handler.AppendLiteral("- ");
					handler.AppendFormatted(component.ComponentId);
					handler.AppendLiteral(":");
					stringBuilder2.AppendLine(ref handler);
					stringBuilder.Append(component.ToLogString(depth + 1, indentChars));
				}
				return stringBuilder.ToString();
			}
		}

		private const string BlobPropertyName = "MinionLibComponentStateBlob";
	}
	public static class LocHelper
	{
		public static void AddMany(this LocString loc, IEnumerable<DynamicVar> value)
		{
			foreach (DynamicVar item in value)
			{
				loc.Add(item);
			}
		}

		public static void AddMany(this LocString loc, DynamicVarSet value)
		{
			value.AddTo(loc);
		}

		public static void AddMany(this LocString loc, IReadOnlyDictionary<string, object> value)
		{
			foreach (var (text2, obj2) in value)
			{
				loc.AddObj(text2, obj2);
			}
		}
	}
}
namespace MinionLib.Component.Core
{
	public readonly record struct ApplyComponentOptions(bool AllowMerge = true, bool UseSubtractiveMerge = false, bool IsUpgrade = false, Dictionary<string, object?>? Extra = null);
	public static class CardComponentRegistry
	{
		private static readonly Dictionary<string, Type> IdToType = new Dictionary<string, Type>();

		private static readonly Dictionary<string, Func<ICardComponent>> IdToFactory = new Dictionary<string, Func<ICardComponent>>();

		public static void Register(string componentId, Type componentType, Func<ICardComponent> factory)
		{
			if (string.IsNullOrWhiteSpace(componentId))
			{
				throw new ArgumentException("Component id cannot be null or empty", "componentId");
			}
			ArgumentNullException.ThrowIfNull(componentType, "componentType");
			ArgumentNullException.ThrowIfNull(factory, "factory");
			if (IdToType.TryGetValue(componentId, out Type value))
			{
				throw new InvalidOperationException($"Duplicate component id '{componentId}' for {componentType.FullName} and {value.FullName}");
			}
			StringIdPool.Register(componentId);
			IdToType[componentId] = componentType;
			IdToFactory[componentId] = factory;
		}

		public static ICardComponent Create(string componentId)
		{
			if (!IdToFactory.TryGetValue(componentId, out Func<ICardComponent> value))
			{
				throw new InvalidOperationException("Unknown component id '" + componentId + "'");
			}
			ICardComponent cardComponent = value();
			return cardComponent ?? throw new InvalidOperationException("Factory returned null for component id '" + componentId + "'");
		}
	}
	public static class CardComponentStateSerializer
	{
		public static int[] Serialize(IReadOnlyList<ICardComponent> components)
		{
			ArrayBufferWriter<byte> arrayBufferWriter = new ArrayBufferWriter<byte>();
			SerializationUtils.WriteCount(arrayBufferWriter, components.Count);
			foreach (ICardComponent component in components)
			{
				SerializationUtils.WriteString(arrayBufferWriter, component.ComponentId);
				SerializationUtils.WriteSerializableBlock(arrayBufferWriter, component);
			}
			return SerializationUtils.ToIntArray(arrayBufferWriter.WrittenSpan);
		}

		public static List<ICardComponent> Deserialize(int[] state, IComponentsCardModel? owner)
		{
			if (!SerializationUtils.TryFromIntArray(state, out byte[] bytes) || bytes.Length == 0)
			{
				return new List<ICardComponent>();
			}
			ReadOnlySpan<byte> reader = new ReadOnlySpan<byte>(bytes);
			if (!SerializationUtils.TryReadCount(ref reader, out var count))
			{
				return new List<ICardComponent>();
			}
			List<ICardComponent> list = new List<ICardComponent>(count);
			string value;
			for (int i = 0; i < count && SerializationUtils.TryReadString(ref reader, out value) && !string.IsNullOrWhiteSpace(value); i++)
			{
				ICardComponent cardComponent;
				try
				{
					cardComponent = CardComponentRegistry.Create(value);
				}
				catch (Exception)
				{
					if (!SerializationUtils.TrySkipObjectBlock(ref reader))
					{
						break;
					}
					continue;
				}
				if (SerializationUtils.TryReadSerializableBlock(ref reader, cardComponent))
				{
					if (owner != null)
					{
						cardComponent.Attach(owner, isInternal: true);
					}
					list.Add(cardComponent);
				}
			}
			return list;
		}

		public static ICardComponent DeepClone(ICardComponent component)
		{
			IComponentsCardModel componentsCard = component.ComponentsCard;
			return Deserialize(Serialize(new <>z__ReadOnlySingleElementList<ICardComponent>(component)), componentsCard).FirstOrDefault() ?? throw new InvalidOperationException("Failed to clone component " + component.GetType().FullName);
		}
	}
	[AttributeUsage(AttributeTargets.Method)]
	public class ComponentDelegateAttribute : Attribute
	{
		public ComponentDelegateAttribute()
		{
		}

		public ComponentDelegateAttribute(string name)
		{
		}

		public ComponentDelegateAttribute(string @namespace, string name)
		{
		}
	}
	internal static class ComponentDescriptionRawCache
	{
		private static readonly Dictionary<string, string> RawByLocEntryKey = new Dictionary<string, string>();

		public static bool TryGet(string locEntryKey, out string rawText)
		{
			lock (RawByLocEntryKey)
			{
				return RawByLocEntryKey.TryGetValue(locEntryKey, out rawText);
			}
		}

		public static bool Contains(string locEntryKey)
		{
			lock (RawByLocEntryKey)
			{
				return RawByLocEntryKey.ContainsKey(locEntryKey);
			}
		}

		public static void Set(string locEntryKey, string rawText)
		{
			lock (RawByLocEntryKey)
			{
				RawByLocEntryKey[locEntryKey] = rawText;
			}
		}

		public static void Clear()
		{
			lock (RawByLocEntryKey)
			{
				RawByLocEntryKey.Clear();
			}
		}
	}
	public enum ComponentPhase
	{
		Init,
		Prime,
		Prefix,
		Core,
		Postfix,
		Final
	}
	public static class ComponentPhaseExtensions
	{
		public static ComponentPhase NextPhase(this ComponentPhase phase)
		{
			return phase switch
			{
				ComponentPhase.Init => ComponentPhase.Prime, 
				ComponentPhase.Prime => ComponentPhase.Prefix, 
				ComponentPhase.Prefix => ComponentPhase.Core, 
				ComponentPhase.Core => ComponentPhase.Postfix, 
				ComponentPhase.Postfix => ComponentPhase.Final, 
				ComponentPhase.Final => ComponentPhase.Final, 
				_ => throw new ArgumentOutOfRangeException("phase", phase, null), 
			};
		}
	}
	public sealed class ComponentContext(ComponentPhase phase)
	{
		public ComponentPhase Phase { get; set; } = phase;

		public Dictionary<string, object> State { get; } = new Dictionary<string, object>();

		public void MoveNextPhase()
		{
			Phase = Phase.NextPhase();
		}
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class ComponentStateAttribute : Attribute
	{
	}
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class ComponentStateAttribute<T>(params object[] parameters) : ComponentStateAttribute where T : DynamicVar
	{
		private readonly object[] _parameters = parameters;
	}
	public static class DelegateRegistry
	{
		private static readonly Dictionary<(string name, Type type), Delegate> Delegates = new Dictionary<(string, Type), Delegate>();

		public static void Register<T>(string name, T del) where T : Delegate
		{
			StringIdPool.Register(name);
			Delegates[(name, typeof(T))] = del;
		}

		public static T? Get<T>(string name) where T : Delegate
		{
			if (Delegates.TryGetValue((name, typeof(T)), out Delegate value))
			{
				return (T)value;
			}
			return null;
		}
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class LocArgAttribute(string? name = null) : Attribute
	{
		public string? Name { get; } = name;
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class NotLocArgAttribute : Attribute
	{
	}
	[AttributeUsage(AttributeTargets.Property)]
	public class NestedLocStringAttribute : Attribute
	{
	}
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public sealed class NoGeneratedSerializationAttribute : Attribute
	{
	}
	public static class SerializationUtils
	{
		private const byte DecimalIsNegativeFlag = 1;

		private const byte DecimalHasScaleFlag = 2;

		private const byte DecimalHasMidFlag = 4;

		private const byte DecimalHasHighFlag = 8;

		private const byte EmptyStringTag = 1;

		private const byte ShortRawStringTagMin = 3;

		private const byte ShortRawStringTagMax = 15;

		private const byte LongRawStringTag = 17;

		private const byte NullStringTag = byte.MaxValue;

		private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.General);

		public static void WriteObjectBlock(ArrayBufferWriter<byte> writer, Action<ArrayBufferWriter<byte>> serializePayload)
		{
			ArrayBufferWriter<byte> arrayBufferWriter = new ArrayBufferWriter<byte>();
			serializePayload(arrayBufferWriter);
			WriteCount(writer, arrayBufferWriter.WrittenCount);
			WriteBytes(writer, arrayBufferWriter.WrittenSpan);
		}

		public static bool TryReadObjectBlock(ref ReadOnlySpan<byte> reader, out ReadOnlySpan<byte> payload)
		{
			payload = default(ReadOnlySpan<byte>);
			if (!TryReadCount(ref reader, out var count) || reader.Length < count)
			{
				return false;
			}
			payload = reader.Slice(0, count);
			reader = reader.Slice(count);
			return true;
		}

		public static bool TrySkipObjectBlock(ref ReadOnlySpan<byte> reader)
		{
			ReadOnlySpan<byte> payload;
			return TryReadObjectBlock(ref reader, out payload);
		}

		public static void WriteSerializableBlock(ArrayBufferWriter<byte> writer, IGeneratedBinarySerializable serializable)
		{
			WriteObjectBlock(writer, serializable.Serialize);
		}

		public static bool TryReadSerializableBlock(ref ReadOnlySpan<byte> reader, IGeneratedBinarySerializable serializable)
		{
			if (!TryReadObjectBlock(ref reader, out var payload))
			{
				return false;
			}
			ReadOnlySpan<byte> reader2 = payload;
			if (!serializable.Deserialize(ref reader2))
			{
				return false;
			}
			return reader2.IsEmpty;
		}

		public static int[] ToIntArray(ReadOnlySpan<byte> bytes)
		{
			int length = bytes.Length;
			int[] array = new int[(length + 3) / 4 + 1];
			array[0] = length;
			if (length > 0)
			{
				Span<byte> destination = MemoryMarshal.AsBytes(array.AsSpan(1));
				bytes.CopyTo(destination);
			}
			return array;
		}

		public static bool TryFromIntArray(int[]? source, out byte[] bytes)
		{
			bytes = Array.Empty<byte>();
			if (source == null || source.Length == 0)
			{
				return true;
			}
			int num = source[0];
			int num2 = (source.Length - 1) * 4;
			if (num < 0 || num > num2)
			{
				return false;
			}
			bytes = new byte[num];
			if (num == 0)
			{
				return true;
			}
			MemoryMarshal.AsBytes(source.AsSpan(1)).Slice(0, num).CopyTo(bytes);
			return true;
		}

		public static void WriteCount(ArrayBufferWriter<byte> writer, int count)
		{
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "count must be non-negative");
			}
			WriteUInt32(writer, (uint)count);
		}

		public static bool TryReadCount(ref ReadOnlySpan<byte> reader, out int count)
		{
			count = 0;
			if (!TryReadUInt32(ref reader, out var value))
			{
				return false;
			}
			if (value > int.MaxValue)
			{
				return false;
			}
			count = (int)value;
			return true;
		}

		public static void WriteBoolean(ArrayBufferWriter<byte> writer, bool value)
		{
			WriteByte(writer, value ? ((byte)1) : ((byte)0));
		}

		public static bool TryReadBoolean(ref ReadOnlySpan<byte> reader, out bool value)
		{
			value = false;
			if (reader.Length < 1)
			{
				return false;
			}
			byte b = reader[0];
			if (b > 1)
			{
				return false;
			}
			value = b == 1;
			reader = reader.Slice(1);
			return true;
		}

		public static void WriteByte(ArrayBufferWriter<byte> writer, byte value)
		{
			writer.GetSpan(1)[0] = value;
			writer.Advance(1);
		}

		public static bool TryReadByte(ref ReadOnlySpan<byte> reader, out byte value)
		{
			value = 0;
			if (reader.Length < 1)
			{
				return false;
			}
			value = reader[0];
			reader = reader.Slice(1);
			return true;
		}

		public static void WriteInt16(ArrayBufferWriter<byte> writer, short value, bool constantLength = false)
		{
			if (constantLength)
			{
				BinaryPrimitives.WriteInt16LittleEndian(writer.GetSpan(2), value);
				writer.Advance(2);
			}
			else
			{
				WriteInt32(writer, value);
			}
		}

		public static bool TryReadInt16(ref ReadOnlySpan<byte> reader, out short value, bool constantLength = false)
		{
			value = 0;
			if (constantLength)
			{
				if (reader.Length < 2)
				{
					return false;
				}
				value = BinaryPrimitives.ReadInt16LittleEndian(reader);
				reader = reader.Slice(2);
				return true;
			}
			if (!TryReadInt32(ref reader, out var value2))
			{
				return false;
			}
			if (value2 < -32768 || value2 > 32767)
			{
				return false;
			}
			value = (short)value2;
			return true;
		}

		public static void WriteUInt16(ArrayBufferWriter<byte> writer, ushort value, bool constantLength = false)
		{
			if (constantLength)
			{
				BinaryPrimitives.WriteUInt16LittleEndian(writer.GetSpan(2), value);
				writer.Advance(2);
			}
			else
			{
				WriteUInt32(writer, value);
			}
		}

		public static bool TryReadUInt16(ref ReadOnlySpan<byte> reader, out ushort value, bool constantLength = false)
		{
			value = 0;
			if (constantLength)
			{
				if (reader.Length < 2)
				{
					return false;
				}
				value = BinaryPrimitives.ReadUInt16LittleEndian(reader);
				reader = reader.Slice(2);
				return true;
			}
			if (!TryReadUInt32(ref reader, out var value2))
			{
				return false;
			}
			if (value2 > 65535)
			{
				return false;
			}
			value = (ushort)value2;
			return true;
		}

		public static void WriteInt32(ArrayBufferWriter<byte> writer, int value, bool constantLength = false)
		{
			if (constantLength)
			{
				BinaryPrimitives.WriteInt32LittleEndian(writer.GetSpan(4), value);
				writer.Advance(4);
			}
			else
			{
				uint value2 = (uint)((value << 1) ^ (value >> 31));
				WriteVarUInt32(writer, value2);
			}
		}

		public static bool TryReadInt32(ref ReadOnlySpan<byte> reader, out int value, bool constantLength = false)
		{
			value = 0;
			if (constantLength)
			{
				if (reader.Length < 4)
				{
					return false;
				}
				value = BinaryPrimitives.ReadInt32LittleEndian(reader);
				reader = reader.Slice(4);
				return true;
			}
			if (!TryReadVarUInt32(ref reader, out var value2))
			{
				return false;
			}
			value = (int)((value2 >> 1) ^ (0 - (value2 & 1)));
			return true;
		}

		public static void WriteUInt32(ArrayBufferWriter<byte> writer, uint value, bool constantLength = false)
		{
			if (constantLength)
			{
				BinaryPrimitives.WriteUInt32LittleEndian(writer.GetSpan(4), value);
				writer.Advance(4);
			}
			else
			{
				WriteVarUInt32(writer, value);
			}
		}

		public static bool TryReadUInt32(ref ReadOnlySpan<byte> reader, out uint value, bool constantLength = false)
		{
			value = 0u;
			if (constantLength)
			{
				if (reader.Length < 4)
				{
					return false;
				}
				value = BinaryPrimitives.ReadUInt32LittleEndian(reader);
				reader = reader.Slice(4);
				return true;
			}
			return TryReadVarUInt32(ref reader, out value);
		}

		public static void WriteInt64(ArrayBufferWriter<byte> writer, long value, bool constantLength = false)
		{
			if (constantLength)
			{
				BinaryPrimitives.WriteInt64LittleEndian(writer.GetSpan(8), value);
				writer.Advance(8);
			}
			else
			{
				ulong value2 = (ulong)((value << 1) ^ (value >> 63));
				WriteVarUInt64(writer, value2);
			}
		}

		public static bool TryReadInt64(ref ReadOnlySpan<byte> reader, out long value, bool constantLength = false)
		{
			value = 0L;
			if (constantLength)
			{
				if (reader.Length < 8)
				{
					return false;
				}
				value = BinaryPrimitives.ReadInt64LittleEndian(reader);
				reader = reader.Slice(8);
				return true;
			}
			if (!TryReadVarUInt64(ref reader, out var value2))
			{
				return false;
			}
			value = (long)((value2 >> 1) ^ (0L - (value2 & 1)));
			return true;
		}

		public static void WriteUInt64(ArrayBufferWriter<byte> writer, ulong value, bool constantLength = false)
		{
			if (constantLength)
			{
				BinaryPrimitives.WriteUInt64LittleEndian(writer.GetSpan(8), value);
				writer.Advance(8);
			}
			else
			{
				WriteVarUInt64(writer, value);
			}
		}

		public static bool TryReadUInt64(ref ReadOnlySpan<byte> reader, out ulong value, bool constantLength = false)
		{
			value = 0uL;
			if (constantLength)
			{
				if (reader.Length < 8)
				{
					return false;
				}
				value = BinaryPrimitives.ReadUInt64LittleEndian(reader);
				reader = reader.Slice(8);
				return true;
			}
			return TryReadVarUInt64(ref reader, out value);
		}

		private static void WriteVarUInt32(ArrayBufferWriter<byte> writer, uint value)
		{
			while (value >= 128)
			{
				WriteByte(writer, (byte)(value | 0x80));
				value >>= 7;
			}
			WriteByte(writer, (byte)value);
		}

		private static bool TryReadVarUInt32(ref ReadOnlySpan<byte> reader, out uint value)
		{
			value = 0u;
			int num = 0;
			do
			{
				if (reader.Length == 0)
				{
					return false;
				}
				byte b = reader[0];
				reader = reader.Slice(1);
				value |= (uint)((b & 0x7F) << num);
				if ((b & 0x80) == 0)
				{
					return true;
				}
				num += 7;
			}
			while (num < 35);
			return false;
		}

		private static void WriteVarUInt64(ArrayBufferWriter<byte> writer, ulong value)
		{
			while (value >= 128)
			{
				WriteByte(writer, (byte)(value | 0x80));
				value >>= 7;
			}
			WriteByte(writer, (byte)value);
		}

		private static bool TryReadVarUInt64(ref ReadOnlySpan<byte> reader, out ulong value)
		{
			value = 0uL;
			int num = 0;
			do
			{
				if (reader.Length == 0)
				{
					return false;
				}
				byte b = reader[0];
				reader = reader.Slice(1);
				value |= ((ulong)b & 0x7FuL) << num;
				if ((b & 0x80) == 0)
				{
					return true;
				}
				num += 7;
			}
			while (num < 70);
			return false;
		}

		public static void WriteSingle(ArrayBufferWriter<byte> writer, float value)
		{
			WriteInt32(writer, BitConverter.SingleToInt32Bits(value), constantLength: true);
		}

		public static bool TryReadSingle(ref ReadOnlySpan<byte> reader, out float value)
		{
			value = 0f;
			if (!TryReadInt32(ref reader, out var value2, constantLength: true))
			{
				return false;
			}
			value = BitConverter.Int32BitsToSingle(value2);
			return true;
		}

		public static void WriteDouble(ArrayBufferWriter<byte> writer, double value)
		{
			WriteInt64(writer, BitConverter.DoubleToInt64Bits(value), constantLength: true);
		}

		public static bool TryReadDouble(ref ReadOnlySpan<byte> reader, out double value)
		{
			value = 0.0;
			if (!TryReadInt64(ref reader, out var value2, constantLength: true))
			{
				return false;
			}
			value = BitConverter.Int64BitsToDouble(value2);
			return true;
		}

		public static void WriteDecimal(ArrayBufferWriter<byte> writer, decimal value)
		{
			Span<int> destination = stackalloc int[4];
			decimal.GetBits(value, destination);
			uint value2 = (uint)destination[0];
			uint num = (uint)destination[1];
			uint num2 = (uint)destination[2];
			int num3 = destination[3];
			bool flag = (num3 & int.MinValue) != 0;
			byte b = (byte)((num3 >>> 16) & 0x7F);
			byte b2 = 0;
			if (flag)
			{
				b2 |= 1;
			}
			if (b > 0)
			{
				b2 |= 2;
			}
			if (num != 0)
			{
				b2 |= 4;
			}
			if (num2 != 0)
			{
				b2 |= 8;
			}
			WriteByte(writer, b2);
			if (b > 0)
			{
				WriteByte(writer, b);
			}
			WriteUInt32(writer, value2);
			if (num != 0)
			{
				WriteUInt32(writer, num);
			}
			if (num2 != 0)
			{
				WriteUInt32(writer, num2);
			}
		}

		public static bool TryReadDecimal(ref ReadOnlySpan<byte> reader, out decimal value)
		{
			value = 0m;
			if (!TryReadByte(ref reader, out var value2))
			{
				return false;
			}
			bool isNegative = (value2 & 1) != 0;
			bool num = (value2 & 2) != 0;
			bool flag = (value2 & 4) != 0;
			bool flag2 = (value2 & 8) != 0;
			byte value3 = 0;
			if (num && !TryReadByte(ref reader, out value3))
			{
				return false;
			}
			if (!TryReadUInt32(ref reader, out var value4))
			{
				return false;
			}
			uint value5 = 0u;
			if (flag && !TryReadUInt32(ref reader, out value5))
			{
				return false;
			}
			uint value6 = 0u;
			if (flag2 && !TryReadUInt32(ref reader, out value6))
			{
				return false;
			}
			value = new decimal((int)value4, (int)value5, (int)value6, isNegative, value3);
			return true;
		}

		public static void WriteString(ArrayBufferWriter<byte> writer, string? value)
		{
			if (value == null)
			{
				WriteByte(writer, byte.MaxValue);
				return;
			}
			if (value == "")
			{
				WriteByte(writer, 1);
				return;
			}
			if (value.Length < 8)
			{
				int byteCount = Encoding.UTF8.GetByteCount(value);
				if (byteCount < 8)
				{
					WriteByte(writer, (byte)((byteCount << 1) | 1));
					Span<byte> span = writer.GetSpan(byteCount);
					int bytes = Encoding.UTF8.GetBytes(value.AsSpan(), span);
					writer.Advance(bytes);
					return;
				}
			}
			if (StringIdPool.TryGetId(value, out var id))
			{
				WriteUInt64(writer, id, constantLength: true);
				return;
			}
			WriteByte(writer, 17);
			int byteCount2 = Encoding.UTF8.GetByteCount(value);
			WriteCount(writer, byteCount2);
			Span<byte> span2 = writer.GetSpan(byteCount2);
			int bytes2 = Encoding.UTF8.GetBytes(value.AsSpan(), span2);
			writer.Advance(bytes2);
		}

		public static bool TryReadString(ref ReadOnlySpan<byte> reader, out string? value)
		{
			value = null;
			if (reader.IsEmpty)
			{
				return false;
			}
			byte b = reader[0];
			if ((b & 1) == 0)
			{
				if (reader.Length < 8)
				{
					return false;
				}
				ulong id = BinaryPrimitives.ReadUInt64LittleEndian(reader);
				reader = reader.Slice(8);
				return StringIdPool.TryGetString(id, out value);
			}
			switch (b)
			{
			case 1:
				value = "";
				reader = reader.Slice(1);
				return true;
			case byte.MaxValue:
				value = null;
				reader = reader.Slice(1);
				return true;
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
			case 9:
			case 10:
			case 11:
			case 12:
			case 13:
			case 14:
			case 15:
			{
				reader = reader.Slice(1);
				int num = b >> 1;
				if (reader.Length < num)
				{
					return false;
				}
				value = Encoding.UTF8.GetString(reader.Slice(0, num));
				reader = reader.Slice(num);
				return true;
			}
			case 17:
			{
				reader = reader.Slice(1);
				if (!TryReadCount(ref reader, out var count))
				{
					return false;
				}
				if (reader.Length < count)
				{
					return false;
				}
				value = ((count == 0) ? "" : Encoding.UTF8.GetString(reader.Slice(0, count)));
				reader = reader.Slice(count);
				return true;
			}
			default:
				return false;
			}
		}

		public static void WriteJson<T>(ArrayBufferWriter<byte> writer, T value)
		{
			WriteString(writer, JsonSerializer.Serialize(value, JsonOptions));
		}

		public static bool TryReadJson<T>(ref ReadOnlySpan<byte> reader, out T value)
		{
			value = default(T);
			if (!TryReadString(ref reader, out string value2) || value2 == null)
			{
				return false;
			}
			try
			{
				T val = JsonSerializer.Deserialize<T>(value2, JsonOptions);
				value = val;
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static void WriteBytes(ArrayBufferWriter<byte> writer, ReadOnlySpan<byte> bytes)
		{
			if (bytes.Length != 0)
			{
				bytes.CopyTo(writer.GetSpan(bytes.Length));
				writer.Advance(bytes.Length);
			}
		}

		public static void WriteIPacketSerializable<T>(ArrayBufferWriter<byte> writer, T value) where T : IPacketSerializable, new()
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Expected O, but got Unknown
			PacketWriter val = new PacketWriter();
			((IPacketSerializable)value/*cast due to constrained. prefix*/).Serialize(val);
			val.ZeroByteRemainder();
			WriteCount(writer, val.BytePosition);
			WriteBytes(writer, val.Buffer.AsSpan(0, val.BytePosition));
		}

		public static bool TryReadIPacketSerializable<T>(ref ReadOnlySpan<byte> reader, out T value) where T : IPacketSerializable, new()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Expected O, but got Unknown
			value = default(T);
			if (!TryReadCount(ref reader, out var count) || reader.Length < count)
			{
				return false;
			}
			ReadOnlySpan<byte> readOnlySpan = reader.Slice(0, count);
			reader = reader.Slice(count);
			try
			{
				PacketReader val = new PacketReader();
				val.Reset(readOnlySpan.ToArray());
				value = new T();
				((IPacketSerializable)value/*cast due to constrained. prefix*/).Deserialize(val);
				return true;
			}
			catch
			{
				return false;
			}
		}
	}
	public static class StringIdPool
	{
		private static readonly Dictionary<ulong, string> IdToString = new Dictionary<ulong, string>();

		private static readonly Dictionary<string, ulong> StringToId = new Dictionary<string, ulong>();

		private static ulong Calculate64BitHash(string str)
		{
			ulong num = 14695981039346656037uL;
			foreach (char c in str)
			{
				num ^= c;
				num *= 1099511628211L;
			}
			return num;
		}

		public static ulong Register(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return 0uL;
			}
			ulong num = Calculate64BitHash(value) << 1;
			if (IdToString.TryGetValue(num, out string value2))
			{
				if (value2 == value)
				{
					return num;
				}
				throw new InvalidOperationException($"StringPool Hash Collision! '{value}' and '{value2}'");
			}
			IdToString[num] = value;
			StringToId[value] = num;
			return num;
		}

		public static bool TryGetId(string value, out ulong id)
		{
			return StringToId.TryGetValue(value, out id);
		}

		public static bool TryGetString(ulong id, [MaybeNullWhen(false)] out string value)
		{
			return IdToString.TryGetValue(id, out value);
		}
	}
	public static class StringIdPoolCollectorPatch
	{
		[HarmonyPatch(typeof(AbstractModel), "InitId")]
		[HarmonyPostfix]
		public static void InitIdPostfix(AbstractModel __instance)
		{
			ModelId id = __instance.Id;
			StringIdPool.Register(id.Category);
			StringIdPool.Register(id.Entry);
			StringIdPool.Register(((object)__instance).GetType().FullName ?? "");
		}
	}
}
namespace MinionLib.Commands
{
	public static class MinionAnimCmd
	{
		private static Tween? _activeTween;

		public static async Task Rearrange(bool animated = true, float duration = 0.25f)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance != null)
			{
				IEnumerable<MinionNodePosition> nodePositions = MinionLayoutManager.CalculateLayout(instance);
				if (animated)
				{
					await AnimatedMove(nodePositions, duration);
				}
				else
				{
					InstantMove(nodePositions);
				}
			}
		}

		public static void InstantMove(IEnumerable<MinionNodePosition> nodePositions)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			foreach (var (val3, position) in nodePositions)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)val3))
				{
					((Control)val3).Position = position;
				}
			}
		}

		public static async Task AnimatedMove(IEnumerable<MinionNodePosition> nodePositions, float duration = 0.25f)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			if (instance == null)
			{
				return;
			}
			if (_activeTween != null && _activeTween.IsValid())
			{
				((GodotObject)_activeTween).EmitSignal(StringName.op_Implicit("finished"), Array.Empty<Variant>());
				_activeTween.Kill();
			}
			Tween val = ((Node)instance).CreateTween();
			val.SetParallel(true);
			foreach (var (val4, val5) in nodePositions)
			{
				if (GodotObject.IsInstanceValid((GodotObject)(object)val4))
				{
					val.TweenProperty((GodotObject)(object)val4, NodePath.op_Implicit("position"), Variant.op_Implicit(val5), (double)duration).SetTrans((TransitionType)4).SetEase((EaseType)1);
				}
			}
			_activeTween = val;
			await ((GodotObject)instance).ToSignal((GodotObject)(object)val, SignalName.Finished);
		}

		public static async Task PlayBumpAttackAsync(Creature attacker, Creature target, System.Action? onHit = null)
		{
			NCombatRoom instance = NCombatRoom.Instance;
			NCreature val = ((instance != null) ? instance.GetCreatureNode(attacker) : null);
			NCreature val2 = ((instance != null) ? instance.GetCreatureNode(target) : null);
			if (val == null || val2 == null)
			{
				return;
			}
			Node2D currentBody = val.Visuals.GetCurrentBody();
			if (GodotObject.IsInstanceValid((GodotObject)(object)currentBody))
			{
				Vector2 globalPosition = currentBody.GlobalPosition;
				Vector2 globalPosition2 = ((Node2D)val2.Visuals.VfxSpawnPosition).GlobalPosition;
				Vector2 val3 = globalPosition2 - globalPosition;
				Vector2 val4 = ((Vector2)(ref val3)).Normalized();
				if (val4 == Vector2.Zero)
				{
					val4 = Vector2.Right;
				}
				Vector2 val5 = globalPosition - val4 * 20f;
				Vector2 val6 = globalPosition2 - val4 * 70f;
				Tween val7 = ((Node)currentBody).CreateTween();
				val7.TweenProperty((GodotObject)(object)currentBody, NodePath.op_Implicit("global_position"), Variant.op_Implicit(val5), 0.15000000596046448).SetTrans((TransitionType)1).SetEase((EaseType)1);
				val7.TweenProperty((GodotObject)(object)currentBody, NodePath.op_Implicit("global_position"), Variant.op_Implicit(val6), 0.10000000149011612).SetTrans((TransitionType)5).SetEase((EaseType)0);
				if (onHit != null)
				{
					val7.TweenCallback(Callable.From(onHit));
				}
				val7.TweenProperty((GodotObject)(object)currentBody, NodePath.op_Implicit("global_position"), Variant.op_Implicit(globalPosition), 0.30000001192092896).SetTrans((TransitionType)10).SetEase((EaseType)1);
				if (val7.IsValid())
				{
					await ((GodotObject)currentBody).ToSignal((GodotObject)(object)val7, SignalName.Finished);
				}
			}
		}
	}
	public static class MinionCmd
	{
		public static async Task<Creature> AddMinion<T>(PlayerChoiceContext choiceContext, Player player, MinionSummonOptions options = default(MinionSummonOptions)) where T : MinionModel
		{
			ArgumentNullException.ThrowIfNull(player, "player");
			Creature pet = await PlayerCmd.AddPet<T>(player);
			if (pet.Monster is MinionModel minionModel)
			{
				minionModel.Position = options.Position;
			}
			PetOrderSnapshotManager.TakeSnapshot(player);
			if (pet.Monster is MinionModel minionModel2)
			{
				await minionModel2.OnSummon(choiceContext, player, options);
			}
			MinionAnimCmd.Rearrange();
			return pet;
		}
	}
	public static class PetOrderSnapshotManager
	{
		private sealed class SnapshotEntry
		{
			public List<uint> CombatIds { get; }

			public SnapshotEntry(List<uint> combatIds)
			{
				CombatIds = combatIds;
			}
		}

		private static readonly object Sync = new object();

		private static ConditionalWeakTable<Player, SnapshotEntry> _snapshots = new ConditionalWeakTable<Player, SnapshotEntry>();

		public static void TakeSnapshot(Player player)
		{
			if (player.PlayerCombatState == null)
			{
				return;
			}
			List<uint> combatIds = (from p in player.PlayerCombatState.Pets
				where p.CombatId.HasValue
				select p.CombatId.Value).ToList();
			lock (Sync)
			{
				_snapshots.Remove(player);
				_snapshots.Add(player, new SnapshotEntry(combatIds));
			}
		}

		public static IReadOnlyList<Creature> GetSnapshot(Player player, bool onlyAlive = true, bool includeMissing = true)
		{
			if (player.PlayerCombatState == null)
			{
				return Array.Empty<Creature>();
			}
			IReadOnlyList<Creature> pets = player.PlayerCombatState.Pets;
			ICombatState combatState = player.Creature.CombatState;
			List<uint> list;
			lock (Sync)
			{
				list = (_snapshots.TryGetValue(player, out SnapshotEntry value) ? value.CombatIds.ToList() : new List<uint>());
			}
			if (includeMissing)
			{
				HashSet<uint> hashSet = new HashSet<uint>();
				foreach (uint item in list)
				{
					hashSet.Add(item);
				}
				HashSet<uint> hashSet2 = hashSet;
				foreach (Creature item2 in pets)
				{
					if (item2.CombatId.HasValue && !hashSet2.Contains(item2.CombatId.Value))
					{
						list.Add(item2.CombatId.Value);
						hashSet2.Add(item2.CombatId.Value);
					}
				}
			}
			List<Creature> list2 = new List<Creature>();
			HashSet<uint> hashSet3 = new HashSet<uint>();
			foreach (uint id in list)
			{
				if (hashSet3.Add(id))
				{
					Creature val = ((combatState != null) ? combatState.GetCreature((uint?)id) : null) ?? pets.FirstOrDefault((Creature p) => p.CombatId == id);
					if (val != null && (!onlyAlive || val.IsAlive))
					{
						list2.Add(val);
					}
				}
			}
			if (includeMissing)
			{
				foreach (Creature item3 in pets)
				{
					if (!item3.CombatId.HasValue && (!onlyAlive || item3.IsAlive))
					{
						list2.Add(item3);
					}
				}
			}
			return list2;
		}

		public static void ClearAllSnapshots()
		{
			lock (Sync)
			{
				_snapshots = new ConditionalWeakTable<Player, SnapshotEntry>();
			}
		}
	}
}
namespace MinionLib.Action
{
	public abstract class ActionModel : PowerModel
	{
		private static readonly IHoverTip ActionHoverTip;

		public abstract TargetType TargetType { get; }

		public virtual bool AutoRemoveAtTurnEnd => false;

		public virtual bool DecrementAfterAct => false;

		public virtual bool OnlyRespondIconClick => false;

		protected override IEnumerable<IHoverTip> ExtraHoverTips => new <>z__ReadOnlySingleElementList<IHoverTip>(ActionHoverTip);

		public void Flash()
		{
			((PowerModel)this).Flash();
		}

		public virtual bool CanAct(ICombatState combatState)
		{
			Creature owner = ((PowerModel)this).Owner;
			if ((decimal)((PowerModel)this).Amount > 0m && owner.IsAlive)
			{
				return owner.CombatState == combatState;
			}
			return false;
		}

		public bool IsValidTarget(ICombatState combatState, Creature? target)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (target == null || !target.IsAlive)
			{
				return false;
			}
			if (CustomTargetTypeManager.TryGetCustomTargetType(TargetType, out ICustomTargetType customTargetType))
			{
				return customTargetType.IsValidTarget(this, target);
			}
			return false;
		}

		public IReadOnlyList<Creature> GetValidTargets(ICombatState combatState)
		{
			return combatState.Creatures.Where((Creature target) => IsValidTarget(combatState, target)).ToList();
		}

		public async Task<bool> TryAct(PlayerChoiceContext choiceContext, Creature? target)
		{
			ICombatState combatState = ((PowerModel)this).Owner.CombatState;
			if (combatState == null || !CanAct(combatState))
			{
				return false;
			}
			if ((int)TargetType == 0)
			{
				await ExecuteAct(choiceContext, null);
				return true;
			}
			if (ActionTargetExtensions.IsSingleTarget(TargetType))
			{
				if (!IsValidTarget(combatState, target))
				{
					return false;
				}
				await ExecuteAct(choiceContext, target);
				return true;
			}
			if (GetValidTargets(combatState).Count == 0)
			{
				return false;
			}
			await ExecuteAct(choiceContext, null);
			return true;
		}

		private async Task ExecuteAct(PlayerChoiceContext choiceContext, Creature? target)
		{
			await OnAct(choiceContext, target);
			if (DecrementAfterAct)
			{
				await PowerCmd.Decrement((PowerModel)(object)this);
			}
			if (CombatManager.Instance.IsInProgress)
			{
				await CombatManager.Instance.CheckWinCondition();
			}
		}

		public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if (AutoRemoveAtTurnEnd && ((PowerModel)this).Owner.Side == side && ((PowerModel)this).Amount > 0)
			{
				await PowerCmd.Remove((PowerModel)(object)this);
			}
		}

		protected abstract Task OnAct(PlayerChoiceContext choiceContext, Creature? target);

		static ActionModel()
		{
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Expected O, but got Unknown
			//IL_0024: Expected O, but got Unknown
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			ActionHoverTip = (IHoverTip)(object)new HoverTip(new LocString("static_hover_tips", "MinionLib-Action.title"), new LocString("static_hover_tips", "MinionLib-Action.description"), (Texture2D)null);
		}
	}
	internal static class CreatureActionQueueService
	{
		public static bool TryEnqueue(ActionModel action, Creature? target)
		{
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Invalid comparison between Unknown and I4
			Creature owner = ((PowerModel)action).Owner;
			if (!CombatManager.Instance.IsInProgress || !owner.CombatId.HasValue)
			{
				return false;
			}
			ActionQueueSynchronizer actionQueueSynchronizer = RunManager.Instance.ActionQueueSynchronizer;
			if ((int)actionQueueSynchronizer.CombatState != 1)
			{
				return false;
			}
			if (!CreatureActionQueueThreshold.TryReserve(action))
			{
				return false;
			}
			ExecuteCreatureActionGameAction executeCreatureActionGameAction = new ExecuteCreatureActionGameAction(action, target);
			try
			{
				actionQueueSynchronizer.RequestEnqueue((GameAction)(object)executeCreatureActionGameAction);
			}
			catch
			{
				CreatureActionQueueThreshold.Release(owner.CombatId.Value, ((AbstractModel)action).Id);
				throw;
			}
			return true;
		}
	}
	internal static class CreatureActionQueueThreshold
	{
		private static readonly Dictionary<(uint actorCombatId, ModelId actionId), int> QueuedCount = new Dictionary<(uint, ModelId), int>();

		public static bool IsExhausted(ActionModel action)
		{
			Creature owner = ((PowerModel)action).Owner;
			if (!owner.CombatId.HasValue)
			{
				return true;
			}
			return ((PowerModel)action).Amount <= GetQueuedCount(owner.CombatId.Value, ((AbstractModel)action).Id);
		}

		public static bool TryReserve(ActionModel action)
		{
			Creature owner = ((PowerModel)action).Owner;
			if (!owner.CombatId.HasValue)
			{
				return false;
			}
			uint value = owner.CombatId.Value;
			if (((PowerModel)action).Amount <= GetQueuedCount(value, ((AbstractModel)action).Id))
			{
				return false;
			}
			(uint, ModelId) key = (value, ((AbstractModel)action).Id);
			QueuedCount[key] = GetQueuedCount(value, ((AbstractModel)action).Id) + 1;
			return true;
		}

		public static void Release(uint actorCombatId, ModelId actionId)
		{
			(uint, ModelId) key = (actorCombatId, actionId);
			if (QueuedCount.TryGetValue(key, out var value))
			{
				value--;
				if (value <= 0)
				{
					QueuedCount.Remove(key);
				}
				else
				{
					QueuedCount[key] = value;
				}
			}
		}

		public static void Clear()
		{
			QueuedCount.Clear();
		}

		private static int GetQueuedCount(uint actorCombatId, ModelId actionId)
		{
			if (!QueuedCount.TryGetValue((actorCombatId, actionId), out var value))
			{
				return 0;
			}
			return value;
		}
	}
}
namespace MinionLib.Action.Patches
{
	[HarmonyPatch(typeof(NCreature), "_Ready")]
	public static class ActionClickPatch
	{
		private const string Module = "MinionAction";

		private static readonly HashSet<uint> TargetingActors = new HashSet<uint>();

		[HarmonyPostfix]
		private static void Postfix(NCreature __instance)
		{
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)__instance.Hitbox).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnGuiInput(__instance, inputEvent);
			}), 0u);
		}

		private static void OnGuiInput(NCreature actorNode, InputEvent inputEvent)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			bool flag = val != null && (long)val.ButtonIndex == 1 && ((InputEvent)val).IsReleased();
			InputEventAction val2 = (InputEventAction)(object)((inputEvent is InputEventAction) ? inputEvent : null);
			int num;
			if (val2 != null)
			{
				StringName action = val2.Action;
				if (action == MegaInput.select && ((InputEvent)val2).IsPressed())
				{
					num = (actorNode.Hitbox.HasFocus() ? 1 : 0);
					goto IL_0054;
				}
			}
			num = 0;
			goto IL_0054;
			IL_0054:
			bool flag2 = (byte)num != 0;
			if (flag || flag2)
			{
				NTargetManager instance = NTargetManager.Instance;
				if (!instance.IsInSelection && (!flag || instance.LastTargetingFinishedFrame != ((Node)actorNode).GetTree().GetFrame()))
				{
					TaskHelper.RunSafely(TryUseActionAsync(actorNode, flag2, null));
					((Node)actorNode).GetViewport().SetInputAsHandled();
				}
			}
		}

		public static Task TryUseActionFromIconAsync(NCreature actorNode, ActionModel actionPower, Vector2 position)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return TryUseActionAsync(actorNode, useController: false, actionPower, position);
		}

		private static async Task TryUseActionAsync(NCreature actorNode, bool useController, ActionModel? preferredAction, Vector2? overrideStartPosition = null)
		{
			Creature actor = actorNode.Entity;
			if (!actor.IsAlive || !actor.CombatId.HasValue || !CombatManager.Instance.IsInProgress || CombatManager.Instance.PlayerActionsDisabled || (int)RunManager.Instance.ActionQueueSynchronizer.CombatState != 1 || (actor.PetOwner != null && !LocalContext.IsMe(actor.PetOwner)) || (actor.IsPlayer && !LocalContext.IsMe(actor)) || actor.CombatState == null || actor.CombatState.CurrentSide != actor.Side)
			{
				return;
			}
			ICombatState combatState = actor.CombatState;
			bool triggeredFromIcon = preferredAction != null;
			ActionModel actionPower;
			if (preferredAction != null && ((PowerModel)preferredAction).Owner == actor)
			{
				if (CreatureActionQueueThreshold.IsExhausted(preferredAction))
				{
					return;
				}
				actionPower = preferredAction;
			}
			else
			{
				actionPower = actor.Powers.OfType<ActionModel>().FirstOrDefault((ActionModel power) => !CreatureActionQueueThreshold.IsExhausted(power) && (triggeredFromIcon || !power.OnlyRespondIconClick));
			}
			if (actionPower == null || !actionPower.CanAct(combatState))
			{
				return;
			}
			TargetType targetType = actionPower.TargetType;
			bool flag = ActionTargetExtensions.IsSingleTarget(targetType);
			IReadOnlyList<Creature> validTargets = actionPower.GetValidTargets(combatState);
			if ((int)targetType == 0)
			{
				actionPower.Flash();
				CreatureActionQueueService.TryEnqueue(actionPower, null);
			}
			else if (!flag)
			{
				if (validTargets.Count != 0)
				{
					actionPower.Flash();
					CreatureActionQueueService.TryEnqueue(actionPower, null);
				}
			}
			else if ((int)targetType == 1)
			{
				CreatureActionQueueService.TryEnqueue(actionPower, null);
			}
			else
			{
				if (validTargets.Count == 0)
				{
					return;
				}
				uint actorId = actor.CombatId.Value;
				if (!TargetingActors.Add(actorId))
				{
					return;
				}
				try
				{
					TargetMode val = (TargetMode)(useController ? 3 : 2);
					Vector2 val2 = (Vector2)(((??)overrideStartPosition) ?? (actorNode.Hitbox.GlobalPosition + actorNode.Hitbox.Size / 2f));
					((PowerModel)actionPower).StartPulsing();
					if (CustomTargetTypeManager.IsCustomTargetType(targetType) && CustomTargetTypeManager.TryGetCustomTargetType(targetType, out ICustomTargetType customTargetType))
					{
						NTargetManager.Instance.StartTargeting(MinionTargetTypes.AnyCreature, val2, val, (Func<bool>)(() => !GodotObject.IsInstanceValid((GodotObject)(object)actorNode) || !actor.IsAlive), (Func<Node, bool>)delegate(Node node)
						{
							NCreature val4 = (NCreature)(object)((node is NCreature) ? node : null);
							if (val4 == null)
							{
								return false;
							}
							Creature entity2 = val4.Entity;
							return customTargetType.IsValidTarget(actionPower, entity2);
						});
					}
					else
					{
						NTargetManager.Instance.StartTargeting(targetType, val2, val, (Func<bool>)(() => !GodotObject.IsInstanceValid((GodotObject)(object)actorNode) || !actor.IsAlive), (Func<Node, bool>)null);
					}
					Node obj = await NTargetManager.Instance.SelectionFinished();
					NCreature val3 = (NCreature)(object)((obj is NCreature) ? obj : null);
					if (val3 != null)
					{
						Creature entity = val3.Entity;
						if (actionPower.IsValidTarget(combatState, entity))
						{
							CreatureActionQueueService.TryEnqueue(actionPower, entity);
						}
					}
				}
				finally
				{
					((PowerModel)actionPower).StopPulsing();
					TargetingActors.Remove(actorId);
				}
			}
		}
	}
	[HarmonyPatch(typeof(NPower), "_Ready")]
	public static class ActionPowerIconClickPatch
	{
		private const string Module = "MinionAction";

		[HarmonyPostfix]
		private static void Postfix(NPower __instance)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			((GodotObject)__instance).Connect(SignalName.GuiInput, Callable.From<InputEvent>((Action<InputEvent>)delegate(InputEvent inputEvent)
			{
				OnPowerGuiInput(__instance, inputEvent);
			}), 0u);
		}

		private static void OnPowerGuiInput(NPower powerNode, InputEvent inputEvent)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Invalid comparison between Unknown and I8
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0076: Unknown result type (might be due to invalid IL or missing references)
			InputEventMouseButton val = (InputEventMouseButton)(object)((inputEvent is InputEventMouseButton) ? inputEvent : null);
			if (val != null && (long)val.ButtonIndex == 1 && ((InputEvent)val).IsReleased() && !NTargetManager.Instance.IsInSelection && powerNode.Model is ActionModel actionModel)
			{
				NCombatRoom instance = NCombatRoom.Instance;
				NCreature val2 = ((instance != null) ? instance.GetCreatureNode(((PowerModel)actionModel).Owner) : null);
				if (val2 != null)
				{
					Vector2 position = ((Control)powerNode).GlobalPosition + new Vector2(20f, 20f);
					TaskHelper.RunSafely(ActionClickPatch.TryUseActionFromIconAsync(val2, actionModel, position));
					((Node)powerNode).GetViewport().SetInputAsHandled();
				}
			}
		}
	}
}
namespace MinionLib.Action.GameActions
{
	public sealed class ExecuteCreatureActionGameAction : GameAction
	{
		private const string Module = "MinionAction";

		public override ulong OwnerId => Owner.NetId;

		public override GameActionType ActionType => (GameActionType)2;

		private Player Owner { get; }

		private uint ActorCombatId { get; }

		private uint? TargetCombatId { get; }

		private ModelId ActionModelId { get; }

		public ExecuteCreatureActionGameAction(ActionModel action, Creature? target)
		{
			Creature owner = ((PowerModel)action).Owner;
			if (!owner.CombatId.HasValue)
			{
				throw new InvalidOperationException("Cannot enqueue creature action without actor combat id.");
			}
			Player val = ResolveQueueOwner(owner) ?? throw new InvalidOperationException("Cannot enqueue creature action without queue owner.");
			if (target != null && !target.CombatId.HasValue)
			{
				throw new InvalidOperationException("Cannot enqueue creature action with target that has no combat id.");
			}
			Owner = val;
			ActorCombatId = owner.CombatId.Value;
			TargetCombatId = ((target != null) ? target.CombatId : ((uint?)null));
			ActionModelId = ((AbstractModel)action).Id;
		}

		public ExecuteCreatureActionGameAction(Player owner, uint actorCombatId, ModelId actionModelId, uint? targetCombatId)
		{
			Owner = owner;
			ActorCombatId = actorCombatId;
			ActionModelId = actionModelId;
			TargetCombatId = targetCombatId;
		}

		private static Player? ResolveQueueOwner(Creature actor)
		{
			if (actor.PetOwner != null)
			{
				return actor.PetOwner;
			}
			if (actor.Player != null)
			{
				return actor.Player;
			}
			if (actor.CombatState != null)
			{
				return LocalContext.GetMe(actor.CombatState);
			}
			return null;
		}

		protected override async Task ExecuteAction()
		{
			_ = 1;
			try
			{
				ICombatState combatState = Owner.Creature.CombatState;
				if (combatState == null)
				{
					((GameAction)this).Cancel();
					return;
				}
				Creature actor = combatState.GetCreature((uint?)ActorCombatId);
				if (actor == null || !actor.IsAlive)
				{
					((GameAction)this).Cancel();
					return;
				}
				ActionModel action = actor.Powers.OfType<ActionModel>().FirstOrDefault((ActionModel power) => ((AbstractModel)power).Id == ActionModelId);
				if (action == null || ((PowerModel)action).Owner != actor)
				{
					((GameAction)this).Cancel();
					return;
				}
				if (!action.CanAct(combatState))
				{
					((GameAction)this).Cancel();
					return;
				}
				Creature val = null;
				if (TargetCombatId.HasValue)
				{
					val = await combatState.GetCreatureAsync(TargetCombatId, 10.0);
				}
				if (ActionTargetExtensions.IsSingleTarget(action.TargetType))
				{
					if ((int)action.TargetType == 1 && val == null)
					{
						val = actor;
					}
					if (!action.IsValidTarget(combatState, val))
					{
						((GameAction)this).Cancel();
						return;
					}
				}
				else if ((int)action.TargetType != 0 && action.GetValidTargets(combatState).Count == 0)
				{
					((GameAction)this).Cancel();
					return;
				}
				if (!(await action.TryAct((PlayerChoiceContext)new GameActionPlayerChoiceContext((GameAction)(object)this), val)))
				{
					((GameAction)this).Cancel();
				}
			}
			finally
			{
				CreatureActionQueueThreshold.Release(ActorCombatId, ActionModelId);
			}
		}

		public override INetAction ToNetAction()
		{
			return (INetAction)(object)new NetExecuteCreatureActionGameAction
			{
				ActorCombatId = ActorCombatId,
				ActionModelId = ActionModelId,
				TargetCombatId = TargetCombatId
			};
		}

		public override string ToString()
		{
			return $"{"ExecuteCreatureActionGameAction"} owner={((GameAction)this).OwnerId} actor={ActorCombatId} action={ActionModelId.Entry} target={TargetCombatId?.ToString() ?? "null"}";
		}
	}
	public struct NetExecuteCreatureActionGameAction : INetAction, IPacketSerializable
	{
		public uint ActorCombatId;

		public ModelId ActionModelId;

		public uint? TargetCombatId;

		public GameAction ToGameAction(Player player)
		{
			return (GameAction)(object)new ExecuteCreatureActionGameAction(player, ActorCombatId, ActionModelId, TargetCombatId);
		}

		public void Serialize(PacketWriter writer)
		{
			writer.WriteUInt(ActorCombatId, 6);
			PacketWriterExtensions.WriteModelEntry(writer, ActionModelId);
			writer.WriteBool(TargetCombatId.HasValue);
			if (TargetCombatId.HasValue)
			{
				writer.WriteUInt(TargetCombatId.Value, 6);
			}
		}

		public void Deserialize(PacketReader reader)
		{
			ActorCombatId = reader.ReadUInt(6);
			ActionModelId = PacketReaderExtensions.ReadModelIdAssumingType<PowerModel>(reader);
			TargetCombatId = (reader.ReadBool() ? new uint?(reader.ReadUInt(6)) : ((uint?)null));
		}

		public override string ToString()
		{
			return $"{"NetExecuteCreatureActionGameAction"} actor={ActorCombatId} action={ActionModelId.Entry} target={TargetCombatId?.ToString() ?? "null"}";
		}
	}
}
