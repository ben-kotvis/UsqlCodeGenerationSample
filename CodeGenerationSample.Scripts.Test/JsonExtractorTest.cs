using Microsoft.Analytics.Interfaces;
using Microsoft.Analytics.Types.Sql;
using Microsoft.Analytics.UnitTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CodeGenerationSample.Json;
using CodeGenerationSample.Json.Exceptions;

namespace CodeGenerationSample.Scripts.Test
{
    [TestClass]
    public class JsonExtractorTest
    {
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        
        public IRow RowGenerator()
        {
            //generate the schema
            USqlColumn<string> col1 = new USqlColumn<string>("Region");
            USqlColumn<string> col2 = new USqlColumn<string>("MarketName");
            USqlColumn<string> col3 = new USqlColumn<string>("MarketId");
            USqlColumn<string> col4 = new USqlColumn<string>("Produce");
            List<IColumn> columns = new List<IColumn> { col1, col2, col3, col4 };
            USqlSchema schema = new USqlSchema(columns);
            return new USqlRow(schema, null);
        }

        [TestMethod]
        [DeploymentItem(@"InputFiles\MarketLocations.json")]
        public void TestAdvancedPaths()
        {
            IUpdatableRow output = RowGenerator().AsUpdatable();

            using (FileStream stream = new FileStream(@"MarketLocations.json", FileMode.Open))
            {
                //Read input file 
                USqlStreamReader reader = new USqlStreamReader(stream);
                //Run the UDO
                JsonExtractor extractor = new MultiLevelJsonExtractor("MarketLocations[*]", false, "Region", "MarketName", "MarketId", "SalesCategories.Produce");
                List <IRow> result = extractor.Extract(reader, output).ToList();
                //Verify the schema
                Assert.IsTrue(result.Count == 3);
                Assert.IsTrue(result[0].Schema.Count == 4);
                //Verify the result
                Assert.IsTrue(result[0].Get<string>("MarketName") == "Central");
                Assert.IsTrue(result[0].Get<string>("Produce") != null);
            }
        }

        [TestMethod]
        [DeploymentItem(@"InputFiles\MarketLocations.json")]
        public void TestBypassWarning()
        {
            IUpdatableRow output = RowGenerator().AsUpdatable();

            using (FileStream stream = new FileStream(@"MarketLocations.json", FileMode.Open))
            {
                //Read input file 
                USqlStreamReader reader = new USqlStreamReader(stream);

                //Run the UDO
                JsonExtractor extractor = new MultiLevelJsonExtractor("MarketLocations[*]", true, "RegionBogus", "MarketName", "MarketId", "SalesCategories.Produce");

                try
                {
                    List<IRow> result = extractor.Extract(reader, output).ToList();
                }
                catch(PathNotFoundException)
                {
                    Assert.Fail("Exception shouldn't be thrown when bypassWarning is set to true");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"InputFiles\MarketLocations.json")]
        [ExpectedException(typeof(PathNotFoundException))]
        public void TestBadPathException()
        {

            IUpdatableRow output = RowGenerator().AsUpdatable();

            using (FileStream stream = new FileStream(@"MarketLocations.json", FileMode.Open))
            {
                //Read input file 
                USqlStreamReader reader = new USqlStreamReader(stream);

                //Run the UDO
                JsonExtractor extractor = new MultiLevelJsonExtractor("MarketLocations[*]", false, "RegionBogus", "MarketName", "MarketId", "SalesCategories.Produce");
                List<IRow> result = extractor.Extract(reader, output).ToList();
            }
        }

    }
}
