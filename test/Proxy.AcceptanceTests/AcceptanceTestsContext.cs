﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.VisualStudio.Threading;

namespace Moq.Proxy
{
    public class AcceptanceTestsContext : IDisposable
    {
        static AdhocWorkspace workspace = new AdhocWorkspace(ProxyGenerator.CreateHost());
        static MetadataReference[] references = ReferencePaths.Paths
                .Select(path => MetadataReference.CreateFromFile(path))
                .ToArray();

        Lazy<Project> csproj;
        Lazy<Project> vbproj;
        AsyncLazy<Compilation> csbuild;
        AsyncLazy<Compilation> vbbuild;

        public AcceptanceTestsContext()
        {
            csproj = new Lazy<Project>(() => workspace.AddProject(ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "cscode",
                "cscode.dll",
                LanguageNames.CSharp,
                compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: references)));

            csbuild = new AsyncLazy<Compilation>(() => csproj.Value.GetCompilationAsync(new CancellationTokenSource(AcceptanceTests.AsyncTimeoutMilliseconds).Token));

            vbproj = new Lazy<Project>(() => workspace.AddProject(ProjectInfo.Create(
                ProjectId.CreateNewId(),
                VersionStamp.Create(),
                "vbcode",
                "vbcode.dll",
                LanguageNames.VisualBasic,
                compilationOptions: new VisualBasicCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
                metadataReferences: references)));

            vbbuild = new AsyncLazy<Compilation>(() => vbproj.Value.GetCompilationAsync(new CancellationTokenSource(AcceptanceTests.AsyncTimeoutMilliseconds).Token));
        }

        public AdhocWorkspace Workspace
            => workspace;

        public Project GetProject(string language)
            => language == LanguageNames.CSharp ? csproj.Value : vbproj.Value;

        public Task<Compilation> GetCompilationAsync(string language, CancellationToken cancellationToken)
            => language == LanguageNames.CSharp ? csbuild.GetValueAsync(cancellationToken) : vbbuild.GetValueAsync(cancellationToken);

        public void Dispose()
        {
            workspace.Dispose();
        }
    }
}
