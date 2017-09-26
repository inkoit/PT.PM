﻿using PT.PM.Common;
using PT.PM.Common.CodeRepository;
using PT.PM.TestUtils;
using PT.PM.Patterns.PatternsRepository;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using PT.PM.Patterns;

namespace PT.PM.Matching.Tests
{
    [TestFixture]
    public class JavaScriptMatchingTests
    {
        [Test]
        public void Match_JavaScriptTestPatterns_MatchedExpected()
        {
            var jsCodeAndPatterns = new Tuple<string, string>[]
            {
                new Tuple<string, string>("document.body.innerHTML=\"<svg/onload=alert(1)>\"", "#.innerHTML=<[\"\"]>"),
                new Tuple<string, string>("document.write(\"\\u003csvg/onload\\u003dalert(1)\\u003e\")", "document.write(<[\"\"]>)"),
                new Tuple<string, string>("$('<svg/onload=alert(1)>')", "$(<[\"\"]>)")
            };
            foreach (var tuple in jsCodeAndPatterns)
            {
                var matchingResults = PatternMatchingUtils.GetMatchings(tuple.Item1, tuple.Item2, Language.JavaScript);
                Assert.AreEqual(1, matchingResults.Length, tuple.Item2 + " doesn't match " + tuple.Item1);
            }
        }

        [Test]
        public void Match_JavaScriptAndPhpPatternInsidePhp_MatchedExpected()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "JavaScriptTestPatternsInsidePhp.php"));
            MatchingResultDto[] matchingResults = PatternMatchingUtils.GetMatchings(code, "#.innerHTML=<[\"\"]>", Language.JavaScript);
            Assert.AreEqual(1, matchingResults.Length);
        }

        [Test]
        public void Match_JavaScriptAndPhpPatternInsidePhp_MatchCorrectPatternDependsOnLanguage()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "JavaScriptTestPatternsInsidePhp.php"));
            MatchingResultDto[] matchingResults;

            matchingResults = PatternMatchingUtils.GetMatchings(code, "#.innerHTML=<[\"\"]>",
                new[] { Language.JavaScript }, new[] { Language.JavaScript });
            Assert.AreEqual(1, matchingResults.Length);

            matchingResults = PatternMatchingUtils.GetMatchings(code, "<[password]> = null",
                new[] { Language.Php }, new[] { Language.Php });
            Assert.AreEqual(1, matchingResults.Length);

            matchingResults = PatternMatchingUtils.GetMatchings(code, "#.innerHTML=<[\"\"]>",
                new[] { Language.Php }, new[] { Language.JavaScript });
            Assert.AreEqual(0, matchingResults.Length);

            matchingResults = PatternMatchingUtils.GetMatchings(code, "<[password]> = null",
                new[] { Language.JavaScript }, new[] { Language.Php });
            Assert.AreEqual(0, matchingResults.Length);
        }

        [Test]
        public void Match_TestPatternsJavaScript_MatchedAllDefault()
        {
            var path = Path.Combine(TestUtility.TestsDataPath, "Patterns.js");
            var sourceCodeRep = new FileCodeRepository(path);
            var patternsRepository = new DefaultPatternRepository();

            var workflow = new Workflow(sourceCodeRep, Language.JavaScript, patternsRepository);
            WorkflowResult workflowResult = workflow.Process();
            MatchingResultDto[] matchingResults = workflowResult.MatchingResults
                .ToDto(workflow.SourceCodeRepository)
                .OrderBy(r => r.PatternKey)
                .ToArray();
            PatternDto[] patternDtos = patternsRepository.GetAll()
                .Where(patternDto => patternDto.Languages.Contains(Language.JavaScript)).ToArray();
            foreach (var dto in patternDtos)
            {
                Assert.Greater(matchingResults.Count(p => p.PatternKey == dto.Key), 0, dto.Description);
            }
        }

        [Test]
        public void Match_PhpInJsInPhp_CorrectMatching()
        {
            string code = File.ReadAllText(Path.Combine(TestUtility.TestsDataPath, "php-js-php.php"));
            var matchingResults = PatternMatchingUtils.GetMatchings(code, "<[GLOBALS|frame_content]>",
                new[] { Language.Php, Language.JavaScript },
                new[] { Language.Php, Language.JavaScript });

            Assert.AreEqual(3, matchingResults.Length);
            Assert.IsTrue(matchingResults[0].MatchedCode.Contains("GLOBAL"));
            Assert.AreEqual(9, matchingResults[0].BeginLine);
            Assert.IsTrue(matchingResults[1].MatchedCode.Contains("frame_content"));
            Assert.AreEqual(10, matchingResults[1].BeginLine);
        }
    }
}
