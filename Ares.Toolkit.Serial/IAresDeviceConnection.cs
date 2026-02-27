using System;
using System.Threading.Tasks;

namespace Ares.Toolkit.Serial;

public interface IAresDeviceConnection : IAsyncDisposable
{
  bool IsOpen { get; }
  string Name { get; }
  void AttemptOpen();
  void Close();
}
