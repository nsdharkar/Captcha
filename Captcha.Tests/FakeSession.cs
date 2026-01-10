using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Captcha.Tests
{
    public class FakeSession : ISession
    {
        private readonly Dictionary<string, byte[]> _char = new();

        public bool IsAvailable => true;
        public string Id => Guid.NewGuid().ToString();
        public IEnumerable<string> Keys => _char.Keys;

        public void Clear() => _char.Clear();
        public void Remove(string key) => _char.Remove(key);
        public void Set(string key, byte[] value) => _char[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _char.TryGetValue(key, out value);

        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
