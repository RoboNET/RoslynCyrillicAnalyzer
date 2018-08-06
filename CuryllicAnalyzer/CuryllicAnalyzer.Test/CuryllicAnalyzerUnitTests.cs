using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using CuryllicAnalyzer;
using CyrillicAnalyzer;

namespace CuryllicAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void TestEmpty()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [TestMethod]
        public void TestClassName()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeNameÆ
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CyrillicAnalyzer",
                Message = String.Format("Type name of '{0}' contains non-ASCII letters (symbol '{1}' at index {2})", "TypeNameÆ", "Æ", 8),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 11, 15)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);
        }

        [TestMethod]
        public void TestMethodName()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void TestMethodÆ()
            {
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CyrillicAnalyzer",
                Message = String.Format("Method name of '{0}' contains non-ASCII letters (symbol '{1}' at index {2})", "TestMethodÆ", "Æ", 10),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 18)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void TestMethod()
            {
            }
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);
        }

        [TestMethod]
        public void TestNamespaceName()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1Ñ
    {
        class TypeName
        {   
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CyrillicAnalyzer",
                Message = String.Format("Namespace name of '{0}' contains non-ASCII letters (symbol '{1}' at index {2})", "ConsoleApplication1Ñ", "Ñ", 19),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 9, 15)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);

            var fixtest1 = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1C
    {
        class TypeName
        {   
        }
    }";
            VerifyCSharpFix(test, fixtest1, 1);
        }

        [TestMethod]
        public void TestFieldName()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            int iæ = 0;
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CyrillicAnalyzer",
                Message = String.Format("Field name of '{0}' contains non-ASCII letters (symbol '{1}' at index {2})", "iæ", "æ", 1),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 17)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            int i = 0;
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);
        }

        [TestMethod]
        public void TestPropertyName()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            int iæ {get; set;}
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CyrillicAnalyzer",
                Message = String.Format("Property name of '{0}' contains non-ASCII letters (symbol '{1}' at index {2})", "iæ", "æ", 1),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 13, 17)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            int i {get; set;}
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);
        }

        [TestMethod]
        public void TestVariableName()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void TestMethod()
            {
                int iû = 0;
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "CyrillicAnalyzer",
                Message = String.Format("Local name of '{0}' contains non-ASCII letters (symbol '{1}' at index {2})", "iû", "û", 1),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                        new DiagnosticResultLocation("Test0.cs", 15, 21)
                    }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            void TestMethod()
            {
                int i = 0;
            }
        }
    }";
            VerifyCSharpFix(test, fixtest, 0);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new CyrillicAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new CyrillicAnalyzer.CyrillicAnalyzer();
        }
    }
}
