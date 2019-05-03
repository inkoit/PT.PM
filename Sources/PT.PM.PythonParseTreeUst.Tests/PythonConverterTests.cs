using NUnit.Framework;
using PT.PM.Common;
using PT.PM.TestUtils;
using System.IO;

namespace PT.PM.PythonParseTreeUst.Tests
{
    [TestFixture]
    class PythonConverterTests
    {
        //TODO: add python2 examples
        [TestCase("python3", "examples")]
        [TestCase("python", "samples")]
        public void Convert_PythonFiles_WithoutErrors(string langFolder, string examplesFolder)
        {
            TestUtility.CheckProject(Path.Combine(TestUtility.GrammarsDirectory, langFolder, examplesFolder),
                Language.Python, Stage.Ust);
        }

        [Test]
        public void Convert_PythonPatterns_MatchedResultsEqual()
        {
            var patternsLogger = new TestLogger();
            TestUtility.CheckFile(Path.Combine(TestUtility.TestsDataPath, "python_patterns.py"), Stage.Match,
                patternsLogger);

            Assert.AreEqual(7, patternsLogger.Matches.Count);
        }
    }
}