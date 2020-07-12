using System;
using System.Collections.Immutable;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EmptyAnalyzer {

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BestAnalyzer : DiagnosticAnalyzer {

        string id = "TEST1337";
        string title = "Test get block bug";
        string description = "Test diagnostic description." ;
        string category = "TestGetBlockBug";
        DiagnosticSeverity severity = DiagnosticSeverity.Error;

        DiagnosticDescriptor m_diagnostic ;
        protected  DiagnosticDescriptor diagnostic => m_diagnostic ?? (m_diagnostic = new DiagnosticDescriptor(id,title,description,category,severity,true));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(diagnostic);


        public override void Initialize(AnalysisContext context) {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(analysisContext => {
                var node = analysisContext.Node;
                try {
                    var block = GetBlock(node);
                    analysisContext.ReportDiagnostic(Diagnostic.Create(diagnostic,node.GetLocation()));
                } catch  (Exception e) {
                    var message = $"Error during analysis:\n{e}";
                    analysisContext.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                                                                            id,
                                                                            "Error in analysis.",
                                                                            message,
                                                                            category,severity,true),
                                                                        node.GetLocation()
                                                          ));
                    File.WriteAllText(Path.Combine(Path.GetDirectoryName(node.SyntaxTree.FilePath),"analzyer-error.txt"),message);
                    Log.Error(message);
                    throw;
                }

            }, SyntaxKind.LocalFunctionStatement);

        }

        SyntaxNode GetBlock(SyntaxNode node) {
            switch (node) {
                case AnonymousMethodExpressionSyntax anonExpr:
                    return anonExpr.Block;
                default:
                    return null;
            }
        }

    }



}
