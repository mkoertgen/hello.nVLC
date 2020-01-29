using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using LibVlcWrapper;
using Microsoft.Win32;

namespace MediaPlayer.Vlc
{
    /// <summary>
    ///     Configuration class for VLC native dependencies
    /// </summary>
    public class VlcConfiguration
    {
        private const string UserEnvRegKey = @"HKEY_CURRENT_USER\Environment";

        private const string SystemEnvRegKey =
            @"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager\Environment";

        private readonly string _homePath;
        private string _libVlcVersion;
        private bool _vlcChecked;

        public Lazy<bool> IsVlcPresent { get; }

        public VlcConfiguration(string homePath = null)
        {
            _homePath = homePath ?? GetDefaultHomePath();
            IsVlcPresent = new Lazy<bool>(InitVlc);
        }

        private string LibVlcVersion
        {
            get
            {
                if (_libVlcVersion != null) return _libVlcVersion;
                // This uses nVLC, cf.: https://www.nuget.org/packages/nVLC/
                var pStr = LibVlcMethods.libvlc_get_version();
                _libVlcVersion = Marshal.PtrToStringAnsi(pStr);
                return _libVlcVersion;
            }
        }

        public void VerifyVlcPresent()
        {
            if (!IsVlcPresent.Value)
                throw new InvalidOperationException("Cannot find VLC directory.");
        }

        private bool InitVlc()
        {
            var vlcPath = Path.Combine(_homePath, "vlc");
            var nativePath = Path.Combine(vlcPath, GetPlatform());

            var isVlcPresent = Directory.Exists(nativePath);
            if (!isVlcPresent)
            {
                Trace.TraceWarning("Cannot find VLC directory.");
                return false;
            }

            if (!_vlcChecked)
            {
                // Prepend native path to environment path, to ensure the right libs are being used.
                var path = Environment.GetEnvironmentVariable("PATH");
                path = string.Concat(nativePath, ";", path);
                Environment.SetEnvironmentVariable("PATH", path);

                Trace.TraceInformation("Using VLC {0} {1} from {2}",
                    LibVlcVersion, Environment.Is64BitProcess ? "x64" : "x86", nativePath);
            }

            _vlcChecked = true;
            return true;
        }

        private static string GetDefaultHomePath()
        {
            var homePath = GetSafeEnv("VLC_HOME");
            if (!string.IsNullOrWhiteSpace(homePath))
                return homePath;

            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();
            var executingAssemblyFile = new Uri(assembly.GetName().CodeBase).LocalPath;
            homePath = Path.GetDirectoryName(executingAssemblyFile);
            return homePath;
        }

        private static string GetPlatform()
        {
            return Environment.Is64BitProcess ? "x64" : "x86";
        }

        private static string GetSafeEnv(string envVar)
        {
            var value = Environment.GetEnvironmentVariable(envVar);
            if (string.IsNullOrWhiteSpace(value))
            {
                value = (string)Registry.GetValue(UserEnvRegKey, envVar, null);
                if (string.IsNullOrWhiteSpace(value))
                    value = (string)Registry.GetValue(SystemEnvRegKey, envVar, null);
            }
            return value ?? string.Empty;
        }
    }
}
