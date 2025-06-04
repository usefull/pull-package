using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Usefull.PullPackage.Entities;

namespace Usefull.PullPackage.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="AssyLoadContext"/>.
    /// </summary>
    public static class AssemblyLoadContextExtensions
    {
        /// <summary>
        /// Returns a collection of the <see cref="Assembly"/> instances loaded in the <see cref="AssemblyLoadContext"/>.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/> from which the collection of loaded assemblies is read.</param>
        /// <returns>The collection of loaded assemblies.</returns>
        public static IEnumerable<Assembly> GetAssemblies(this AssemblyLoadContext ctx)
        {
            try
            {
                return ctx.GetType().GetProperty("Assemblies").GetValue(ctx) as IEnumerable<Assembly>;
            }
            catch
            {
                return [];
            }
        }

        /// <summary>
        /// Builds a <see cref="ScriptOptions"/> based on loaded assemblies from <see cref="AssemblyLoadContext"/>.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/>.</param>
        /// <returns>The <see cref="ScriptOptions"/>.</returns>
        public static ScriptOptions BuildScriptOptions(this AssemblyLoadContext ctx) =>
            ScriptOptions.Default.AddReferences(ctx.GetAssemblies());

        /// <summary>
        /// Searches the type in assemblies which loaded in the <see cref="AssemblyLoadContext"/>.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/> in which the search will be performed.</param>
        /// <param name="name">A name of type to search.</param>
        /// <returns>The search result.</returns>
        /// <exception cref="ArgumentNullException">In case of <paramref name="name"/> is null, empty or blank.</exception>
        public static IEnumerable<FindTypeResult> FindType(this AssemblyLoadContext ctx, string name)
        {
            if (string.IsNullOrEmpty(name)) 
                throw new ArgumentNullException(nameof(name));

            var assemblies = ctx.GetAssemblies();

            return assemblies.Select(a =>
            {
                var result = new FindTypeResult
                {
                    Assembly = a,
                    Name = name
                };

                try
                {
                    result.Type = a.GetType(name);
                }
                catch (Exception ex)
                {
                    result.Error = ex;
                }

                return result;
            });
        }

        /// <summary>
        /// Creates the instance of the specified type.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/>in which assemblies the type will be searched for to create an instance.</param>
        /// <param name="typeName">A type name.</param>
        /// <param name="bindingAttr">A combination of zero or more bit flags that affect the search for the <paramref name="typeName"/> constructor. If <paramref name="bindingAttr"/> is zero, a case-sensitive search for public constructors is conducted.</param>
        /// <param name="binder">An object that uses <paramref name="bindingAttr"/> and args to seek and identify the <paramref name="typeName"/> constructor. If <paramref name="binder"/> is null, the default binder is used.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args"/> is an empty array or <see cref="null"/>, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <param name="culture">Culture-specific information that governs the coercion of <paramref name="args"/> to the formal types declared for the <paramref name="typeName"/> constructor. If <paramref name="culture"/> is <see cref="null"/>, the <see cref="System.Globalization.CultureInfo"/> for the current thread is used.</param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="UrlAttribute"/> object that specifies the URL that is required to activate a remote object.</param>
        /// <returns>The reference to the newly created object.</returns>
        /// <exception cref="AmbiguousMatchException">In case multiple types were found.</exception>
        /// <exception cref="AggregateException">If no type was found.</exception>
        /// <exception cref="ArgumentNullException">In case of <paramref name="typeName"/> is null, empty or blank.</exception>
        public static object CreateInstance(this AssemblyLoadContext ctx, string typeName, BindingFlags bindingAttr, Binder binder, object[] args, System.Globalization.CultureInfo culture, object[] activationAttributes = null)
        {
            var type = ctx.GetOnlyOne(typeName);
            return Activator.CreateInstance(type, bindingAttr, binder, args, culture, activationAttributes);
        }

        /// <summary>
        /// Creates the instance of the specified type.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/>in which assemblies the type will be searched for to create an instance.</param>
        /// <param name="typeName">A type name.</param>
        /// <param name="args">An array of arguments that match in number, order, and type the parameters of the constructor to invoke. If <paramref name="args"/> is an empty array or <see cref="null"/>, the constructor that takes no parameters (the parameterless constructor) is invoked.</param>
        /// <param name="activationAttributes">An array of one or more attributes that can participate in activation. This is typically an array that contains a single <see cref="UrlAttribute"/> object that specifies the URL that is required to activate a remote object.</param>
        /// <returns>The reference to the newly created object.</returns>
        /// <exception cref="AmbiguousMatchException">In case multiple types were found.</exception>
        /// <exception cref="AggregateException">If no type was found.</exception>
        /// <exception cref="ArgumentNullException">In case of <paramref name="typeName"/> is null, empty or blank.</exception>
        public static object CreateInstance(this AssemblyLoadContext ctx, string typeName, object[] args, object[] activationAttributes = null)
        {
            var type = ctx.GetOnlyOne(typeName);
            return Activator.CreateInstance(type, args, activationAttributes);
        }

        /// <summary>
        /// Creates the instance of the specified type.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/>in which assemblies the type will be searched for to create an instance.</param>
        /// <param name="typeName">A type name.</param>
        /// <param name="nonPublic"><see cref="true"/> if a public or nonpublic parameterless constructor can match; <see cref="false"/> if only a public parameterless constructor can match.</param>
        /// <returns>The reference to the newly created object.</returns>
        /// <exception cref="AmbiguousMatchException">In case multiple types were found.</exception>
        /// <exception cref="AggregateException">If no type was found.</exception>
        /// <exception cref="ArgumentNullException">In case of <paramref name="typeName"/> is null, empty or blank.</exception>
        public static object CreateInstance(this AssemblyLoadContext ctx, string typeName, bool nonPublic)
        {
            var type = ctx.GetOnlyOne(typeName);
            return Activator.CreateInstance(type, nonPublic);
        }

        /// <summary>
        /// Creates the instance of the specified type.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/>in which assemblies the type will be searched for to create an instance.</param>
        /// <param name="typeName">A type name.</param>
        /// <returns>The reference to the newly created object.</returns>
        /// <exception cref="AmbiguousMatchException">In case multiple types were found.</exception>
        /// <exception cref="AggregateException">If no type was found.</exception>
        /// <exception cref="ArgumentNullException">In case of <paramref name="typeName"/> is null, empty or blank.</exception>
        public static object CreateInstance(this AssemblyLoadContext ctx, string typeName)
        {
            var type = ctx.GetOnlyOne(typeName);
            return Activator.CreateInstance(type);
        }

        /// <summary>
        /// Returns only one type from those found in loaded assemblies.
        /// </summary>
        /// <param name="ctx">An <see cref="AssemblyLoadContext"/>in which assemblies the type will be searched.</param>
        /// <param name="typeName">A type name.</param>
        /// <returns>The type.</returns>
        /// <exception cref="AmbiguousMatchException">In case multiple types were found.</exception>
        /// <exception cref="AggregateException">If no type was found.</exception>
        /// <exception cref="ArgumentNullException">In case of <paramref name="typeName"/> is null, empty or blank.</exception>
        private static Type GetOnlyOne(this AssemblyLoadContext ctx, string typeName)
        {
            var fr = ctx.FindType(typeName);
            var found = fr.Where(r => r.Type != null);
            if (found.Count() > 1)
                throw new AmbiguousMatchException(Resources.AmbiguousTypeName);
            else if (!found.Any())
                throw new AggregateException(Resources.TypeNotFound, fr.Where(r => r.Error != null).Select(r => r.Error));
            else
                return found.First().Type;
        }
    }
}