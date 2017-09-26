﻿using PT.PM.Matching.PatternsRepository;
using System.Collections.Generic;
using PT.PM.Common;

namespace PT.PM.Matching
{
    public class DslPatternRepository : MemoryPatternsRepository
    {
        private string patternData;
        private HashSet<Language> languages;

        public DslPatternRepository(string patternData, Language language)
            : this(patternData, new [] { language })
        {
        }

        public DslPatternRepository(string patternData, IEnumerable<Language> languages)
        {
            this.patternData = patternData;
            this.languages = new HashSet<Language>(languages);
        }

        protected override List<PatternDto> InitPatterns()
        {
            return new List<PatternDto>()
            {
                new PatternDto
                {
                    Key = "temp",
                    Languages = languages,
                    DataFormat = UstFormat.Dsl,
                    Value = patternData
                }
            };
        }
    }
}
