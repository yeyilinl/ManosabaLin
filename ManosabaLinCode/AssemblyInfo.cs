using System.Runtime.CompilerServices;

// 让测试工程能直接构造 YalisalinFireComponentContext 等 internal 类型，
// 对「余火右键记录编码 / 同步回放」这条联机不变式做无头回归
// （见 ManosabaLin.Tests/Cases/YalisalinFireComponentTests.cs）。
[assembly: InternalsVisibleTo("ManosabaLin_tests")]
