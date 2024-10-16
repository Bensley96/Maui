using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using CommunityToolkit.Maui.SourceGenerators.Extensions;
using CommunityToolkit.Maui.SourceGenerators.Helpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CommunityToolkit.Maui.SourceGenerators.Generators;

// IF you want to perform any change in the pipeline or in the generated code
// add this line right before the `#nullable enable` line
// // Final version: {DateTime.Now}
// Use this as a check, if the DateTime value changes when you change a code
// that has not to do with the generator (changing a code in another class, e.g.)
// then you broke the Incremental behavior of it and it need to be fixed before submit a PR

[Generator]
class TextColorToGenerator : IIncrementalGenerator
{
	const string iTextStyleInterface = "Microsoft.Maui.ITextStyle";
	const string iAnimatableInterface = "Microsoft.Maui.Controls.IAnimatable";
	const string mauiControlsAssembly = "Microsoft.Maui.Controls";
	const string mauiColorFullName = "global::Microsoft.Maui.Graphics.Color";
	const string mauiColorsFullName = "global::Microsoft.Maui.Graphics.Colors";

	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Get All Classes in User Library
		var userGeneratedClassesProvider = context.SyntaxProvider.CreateSyntaxProvider(
			static (syntaxNode, cancellationToken) => syntaxNode is ClassDeclarationSyntax { BaseList: not null },
			static (context, cancellationToken) =>
			{
				var compilation = context.SemanticModel.Compilation;

				var iTextStyleInterfaceSymbol = compilation.GetTypeByMetadataName(iTextStyleInterface);
				var iAnimatableInterfaceSymbol = compilation.GetTypeByMetadataName(iAnimatableInterface);

				if (iTextStyleInterfaceSymbol is null || iAnimatableInterfaceSymbol is null)
				{
					throw new Exception("There's no .NET MAUI referenced in the project.");
				}

				var classSymbol = (INamedTypeSymbol?)context.SemanticModel.GetDeclaredSymbol(context.Node);

				// If the ClassDeclarationSyntax doesn't implements those interfaces we just return null
				if (classSymbol is null
					|| !(classSymbol.AllInterfaces.Contains(iAnimatableInterfaceSymbol, SymbolEqualityComparer.Default)
							&& classSymbol.AllInterfaces.Contains(iTextStyleInterfaceSymbol, SymbolEqualityComparer.Default)))
				{
					return null;
				}

				return classSymbol;
			});

		// Get Microsoft.Maui.Controls Symbols that implements the desired interfaces
		var mauiControlsAssemblySymbolProvider = context.CompilationProvider.Select(
			static (compilation, token) =>
			{
				var iTextStyleInterfaceSymbol = compilation.GetTypeByMetadataName(iTextStyleInterface);
				var iAnimatableInterfaceSymbol = compilation.GetTypeByMetadataName(iAnimatableInterface);

				if (iTextStyleInterfaceSymbol is null || iAnimatableInterfaceSymbol is null)
				{
					throw new Exception("There's no .NET MAUI referenced in the project.");
				}

				var mauiAssembly = default(IAssemblySymbol);
				foreach (var assemblySymbol in compilation.SourceModule.ReferencedAssemblySymbols)
				{
					if (assemblySymbol.Name == mauiControlsAssembly)
					{
						if (mauiAssembly is not null)
						{
							throw new InvalidOperationException("There can only be one reference to the Maui Controls assembly.");
						}

						mauiAssembly = assemblySymbol;
					}
				}
				if (mauiAssembly is null)
				{
					throw new InvalidOperationException("There is no reference to the Maui Controls assembly.");
				}

				var symbols = GetMauiInterfaceImplementors(mauiAssembly, iAnimatableInterfaceSymbol, iTextStyleInterfaceSymbol).Where(static x => x is not null);

				return symbols;
			});


		// Here we Collect all the Classes candidates from the first pipeline
		// Then we merge them with the Maui.Controls that implements the desired interfaces
		// Then we make sure they are unique and the user control doesn't inherit from any Maui control that implements the desired interface already
		// Then we transform the ISymbol to be a type that we can compare and preserve the Incremental behavior of this Source Generator
		var inputs = userGeneratedClassesProvider.Collect()
			.Combine(mauiControlsAssemblySymbolProvider)
			.SelectMany(static (x, _) => Deduplicate(x.Left, x.Right).ToImmutableArray())
			.Select(static (x, _) => GenerateMetadata(x));

		context.RegisterSourceOutput(inputs, Execution);
	}

	static void Execution(SourceProductionContext context, TextStyleClassMetadata textStyleClassMetadata)
	{
		var className = typeof(TextColorToGenerator).FullName;
		var assemblyVersion = typeof(TextColorToGenerator).Assembly.GetName().Version.ToString();


		var source = /* language=C#-test */$$"""
// <auto-generated>
// See: CommunityToolkit.Maui.SourceGenerators.TextColorToGenerator

#pragma warning disable
#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace {{textStyleClassMetadata.Namespace}};

[global::System.CodeDom.Compiler.GeneratedCode("{{className}}", "{{assemblyVersion}}")]
[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
{{textStyleClassMetadata.ClassAcessModifier}} static partial class ColorAnimationExtensions_{{textStyleClassMetadata.ClassName}}
{
	/// <summary>
	/// Animates the TextColor of an <see cref="Microsoft.Maui.ITextStyle"/> to the given color
	/// </summary>
	/// <param name="element"></param>
	/// <param name="color">The target color to animate the <see cref="Microsoft.Maui.ITextStyle.TextColor"/> to</param>
	/// <param name="rate">The time, in milliseconds, between the frames of the animation</param>
	/// <param name="length">The duration, in milliseconds, of the animation</param>
	/// <param name="easing">The easing function to be used in the animation</param>
	/// <param name="token"><see cref="CancellationToken"/></param>
	/// <returns>Value indicating if the animation completed successfully or not</returns>
	public static Task<bool> TextColorTo{{textStyleClassMetadata.GenericArguments}}(this global::{{textStyleClassMetadata.Namespace}}.{{textStyleClassMetadata.ClassName}}{{textStyleClassMetadata.GenericArguments}} element, {{mauiColorFullName}} color, uint rate = 16u, uint length = 250u, Easing? easing = null, CancellationToken token = default)
{{textStyleClassMetadata.GenericConstraints}}
	{
		ArgumentNullException.ThrowIfNull(element);
		ArgumentNullException.ThrowIfNull(color);

		if(element is not Microsoft.Maui.ITextStyle)
			throw new ArgumentException($"Element must implement {nameof(Microsoft.Maui.ITextStyle)}", nameof(element));

		//Although TextColor is defined as not-nullable, it CAN be null
		//If null => set it to Transparent as Animation will crash on null BackgroundColor
		element.TextColor ??= {{mauiColorsFullName}}.Transparent;

		var animationCompletionSource = new TaskCompletionSource<bool>();

		try
		{
			new Animation
			{
				{ 0, 1, GetRedTransformAnimation(element, color.Red) },
				{ 0, 1, GetGreenTransformAnimation(element, color.Green) },
				{ 0, 1, GetBlueTransformAnimation(element, color.Blue) },
				{ 0, 1, GetAlphaTransformAnimation(element, color.Alpha) },
			}
			.Commit(element, nameof(TextColorTo), rate, length, easing, (d, b) => animationCompletionSource.SetResult(true));
		}
		catch (ArgumentException aex)
		{
			//When creating an Animation too early in the lifecycle of the Page, i.e. in the OnAppearing method,
			//the Page might not have an 'IAnimationManager' yet, resulting in an ArgumentException.
			System.Diagnostics.Trace.WriteLine($"{aex.GetType().Name} thrown in {typeof(ColorAnimationExtensions_{{textStyleClassMetadata.ClassName}}).FullName}: {aex.Message}");
			animationCompletionSource.SetResult(false);
		}

		return animationCompletionSource.Task.WaitAsync(token);


		static Animation GetRedTransformAnimation({{textStyleClassMetadata.Namespace}}.{{textStyleClassMetadata.ClassName}}{{textStyleClassMetadata.GenericArguments}} element, float targetRed) =>
			new(v => element.TextColor = element.TextColor.WithRed(v), element.TextColor.Red, targetRed);

		static Animation GetGreenTransformAnimation({{textStyleClassMetadata.Namespace}}.{{textStyleClassMetadata.ClassName}}{{textStyleClassMetadata.GenericArguments}} element, float targetGreen) =>
			new(v => element.TextColor = element.TextColor.WithGreen(v), element.TextColor.Green, targetGreen);

		static Animation GetBlueTransformAnimation({{textStyleClassMetadata.Namespace}}.{{textStyleClassMetadata.ClassName}}{{textStyleClassMetadata.GenericArguments}} element, float targetBlue) =>
			new(v => element.TextColor = element.TextColor.WithBlue(v), element.TextColor.Blue, targetBlue);

		static Animation GetAlphaTransformAnimation({{textStyleClassMetadata.Namespace}}.{{textStyleClassMetadata.ClassName}}{{textStyleClassMetadata.GenericArguments}} element, float targetAlpha) =>
			new(v => element.TextColor = element.TextColor.WithAlpha((float)v), element.TextColor.Alpha, targetAlpha);
	}
}
""";

		SourceStringService.FormatText(ref source);
		context.AddSource($"{textStyleClassMetadata.ClassName}TextColorTo.g.shared.cs", SourceText.From(source, Encoding.UTF8));
	}

	static TextStyleClassMetadata GenerateMetadata(INamedTypeSymbol namedTypeSymbol)
	{
		var accessModifier = mauiControlsAssembly == namedTypeSymbol.ContainingNamespace.ToDisplayString()
			? "internal"
			: GetClassAccessModifier(namedTypeSymbol);

		return new(namedTypeSymbol.Name, accessModifier, namedTypeSymbol.ContainingNamespace.ToDisplayString(), namedTypeSymbol.TypeArguments.GetGenericTypeArgumentsString(), namedTypeSymbol.GetGenericTypeConstraintsAsString());
	}

	static IEnumerable<INamedTypeSymbol> Deduplicate(ImmutableArray<INamedTypeSymbol?> left, IEnumerable<INamedTypeSymbol> right)
	{
		foreach (var leftItem in left)
		{
			if (leftItem is null)
			{
				continue;
			}

			var result = right.ContainsSymbolBaseType(leftItem);
			if (!result)
			{
				yield return leftItem;
			}
		}

		foreach (var rightItem in right)
		{
			yield return rightItem;
		}
	}

	static IEnumerable<INamedTypeSymbol> GetMauiInterfaceImplementors(IAssemblySymbol mauiControlsAssemblySymbolProvider, INamedTypeSymbol iAnimatableSymbol, INamedTypeSymbol itextStyleSymbol)
	{
		return mauiControlsAssemblySymbolProvider.GlobalNamespace.GetNamedTypeSymbols().Where(x => x.AllInterfaces.Contains(itextStyleSymbol, SymbolEqualityComparer.Default)
																									&& x.AllInterfaces.Contains(iAnimatableSymbol, SymbolEqualityComparer.Default));
	}

	static string GetClassAccessModifier(INamedTypeSymbol namedTypeSymbol) => namedTypeSymbol.DeclaredAccessibility switch
	{
		Accessibility.Public => "public",
		Accessibility.Internal => "internal",
		_ => string.Empty
	};

	record TextStyleClassMetadata(string ClassName, string ClassAcessModifier, string Namespace, string GenericArguments, string GenericConstraints);
}