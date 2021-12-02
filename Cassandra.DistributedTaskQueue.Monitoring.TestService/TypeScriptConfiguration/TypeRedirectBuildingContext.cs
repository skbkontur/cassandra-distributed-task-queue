using System;

using JetBrains.Annotations;

using SkbKontur.TypeScript.ContractGenerator;
using SkbKontur.TypeScript.ContractGenerator.Abstractions;
using SkbKontur.TypeScript.ContractGenerator.CodeDom;
using SkbKontur.TypeScript.ContractGenerator.TypeBuilders;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Monitoring.TestService.TypeScriptConfiguration
{
    public class TypeRedirectBuildingContext : TypeBuildingContextBase
    {
        public TypeRedirectBuildingContext([NotNull] ITypeInfo type, [NotNull] TypeScriptType typeScriptType)
            : base(type)
        {
            this.typeScriptType = typeScriptType;
        }

        protected override TypeScriptType ReferenceFromInternal(ITypeInfo type, TypeScriptUnit targetUnit, ITypeGenerator typeGenerator)
        {
            if (type.Equals(Type))
                return typeScriptType;
            throw new ArgumentException($"Expected type {Type}, but got {type}");
        }

        [NotNull]
        private readonly TypeScriptType typeScriptType;
    }
}