using Moq;
using Parse;
using Parse.Core.Internal;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Parse.Test
{
    [TestClass]
    public class CloudTests
    {
        [TestCleanup]
        public void TearDown() => ParseCorePlugins.Instance.Reset();

        [TestMethod]
        [AsyncStateMachine(typeof(CloudTests))]
        public Task TestCloudFunctions()
        {
            Mock<IParseCloudCodeController> mockController = new Mock<IParseCloudCodeController>();
            mockController.Setup(obj => obj.CallFunctionAsync<IDictionary<string, object>>(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object> { ["fosco"] = "ben", ["list"] = new List<object> { 1, 2, 3 } }));

            ParseCorePlugins.Instance = new ParseCorePlugins
            {
                CloudCodeController = mockController.Object,
                CurrentUserController = new Mock<IParseCurrentUserController>().Object
            };

            return ParseCloud.CallFunctionAsync<IDictionary<string, object>>("someFunction", null, CancellationToken.None).ContinueWith(t =>
            {
                Assert.IsFalse(t.IsFaulted);
                Assert.IsFalse(t.IsCanceled);
                Assert.IsInstanceOfType(t.Result, typeof(IDictionary<string, object>));
                Assert.AreEqual("ben", t.Result["fosco"]);
                Assert.IsInstanceOfType(t.Result["list"], typeof(IList<object>));
            });
        }
    }
}
