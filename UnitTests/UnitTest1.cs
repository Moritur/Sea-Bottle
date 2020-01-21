using System;
using System.Threading.Tasks;
using GameControl;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        // this test takes ~30s to complete but on lower end hardware you may have to 
        // comment out one of the for loops
        [TestMethod]
        public void TestMethod1()
        {
            /* 
             *  Test most configurations that make any sense for this game.
             *  User is allowed to configure their game in any way, but 
             *  it doesn't have to always handle extreme cases 
             *  (like super large grid or ships) properly  
             */

            // shot limit can't affect this test's result in any way
            int shotLimit = 10;
            int length = 10;

            for (int i = 1; i < 20; i++)
            {
                for (int a = 0; a < length; a++)
                {
                    for (int b = 0; b < length; b++)
                    {
                        for (int c = 0; c < length; c++)
                        {
                            for (int d = 0; d < length; d++)
                            {
                                for (int e = 0; e < length; e++)
                                {
                                    try
                                    {
                                        new GameController(shotLimit, i, a, b, c, d, e);
                                    }
                                    catch (ArgumentException)
                                    {
                                        // this means GameController detected wrong input properly
                                    }
                                }
                            }
                        }
                }
                }
            }
        }
    }
}
