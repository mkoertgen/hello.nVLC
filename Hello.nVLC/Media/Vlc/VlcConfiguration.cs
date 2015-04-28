using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using LibVlcWrapper;
using Microsoft.Win32;

namespace Hello.nVLC.Media.Vlc
{
    /// <summary>
    /// Configuration class for VLC native dependencies
    /// </summary>
    public class VlcConfiguration
    {
        private string _libVlcVersion;
        private bool _vlcChecked;
        private readonly string _homePath;

        public VlcConfiguration(string homePath = null)
        {
            _homePath = homePath ?? GetDefaultHomePath();
        }

        private string LibVlcVersion
        {
            get
            {
                if (_libVlcVersion != null) return _libVlcVersion;
                var pStr = LibVlcMethods.libvlc_get_version();
                _libVlcVersion = Marshal.PtrToStringAnsi(pStr);
                return _libVlcVersion;
            }
        }

        private bool IsVlcPresent { get; set; }

        public void VerifyVlcPresent()
        {
            if (!_vlcChecked)
                CheckVlc();

            if (!IsVlcPresent)
                throw new InvalidOperationException("Cannot find VLC directory.");
        }

        void CheckVlc()
        {
            var vlcPath = Path.Combine(_homePath, "vlc");
            var nativePath = Path.Combine(vlcPath, GetPlatform());

            IsVlcPresent = Directory.Exists(nativePath);
            if (!IsVlcPresent)
            {
                Trace.TraceWarning("Cannot find VLC directory.");
                return;
            }

            // Prepend native path to environment path, to ensure the right libs are being used.
            var path = Environment.GetEnvironmentVariable("PATH");
            path = String.Concat(nativePath, ";", path);
            Environment.SetEnvironmentVariable("PATH", path);

            Trace.TraceInformation("Using VLC {0} {1} from {2}",
                LibVlcVersion, Environment.Is64BitProcess ? "x64" : "x86", nativePath);

            _vlcChecked = true;
        }

        private static string GetDefaultHomePath()
        {
            var homePath = GetSafeEnv("VLC_HOME");
            if (String.IsNullOrWhiteSpace(homePath))
            {
                var executingAssemblyFile = new Uri(Assembly.GetEntryAssembly().GetName().CodeBase).LocalPath;
                homePath = Path.GetDirectoryName(executingAssemblyFile);
            }
            return homePath;
        }

        private static string GetPlatform() { return Environment.Is64BitProcess ? "x64" : "x86"; }

        private const string UserEnvRegKey = @"HKEY_CURRENT_USER\Environment";
        private const string SystemEnvRegKey = @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

        private static string GetSafeEnv(string envVar)
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            if (String.IsNullOrWhiteSpace(value))
            {
                value = (String)Registry.GetValue(UserEnvRegKey, envVar, null);
                if (String.IsNullOrWhiteSpace(value))
                    value = (String)Registry.GetValue(SystemEnvRegKey, envVar, null);
            }
            return value ?? String.Empty;
        }
    }
}