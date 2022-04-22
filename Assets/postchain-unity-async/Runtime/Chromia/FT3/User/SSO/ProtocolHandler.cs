using Microsoft.Win32;

using UnityEngine;

using System.Diagnostics;
using System.Text;
using System;

namespace Chromia.Postchain.Ft3
{
    public static class ProtocolHandler
    {
        public static void Register(string name)
        {
#if UNITY_STANDALONE_LINUX
            RegisterCustomProtocolUnix(name);
#elif UNITY_STANDALONE_WIN
            RegisterCustomProtocolWindows(name);
#else
            throw new NotImplementedException();
#endif
        }

#if UNITY_STANDALONE_WIN
        private static void RegisterCustomProtocolWindows(string name)
        {
            var location = new StringBuilder(Process.GetCurrentProcess().MainModule.FileName);
            location.Replace("/", "\\ ");

            var keyTest = Registry.CurrentUser.OpenSubKey("Software", true).OpenSubKey("Classes", true);
            RegistryKey key = keyTest.CreateSubKey(name);
            key.SetValue("URL Protocol", "");
            key.CreateSubKey(@"shell\open\command").SetValue("", @$"""{location.ToString()}""" + "-batchmode -nographics" + @" ""%1""");
        }
#endif

#if UNITY_STANDALONE_LINUX
        private static void RegisterCustomProtocolUnix(string name)
        {
            var location = new StringBuilder(Process.GetCurrentProcess().MainModule.FileName);
            location.Replace(" ", "\\ ");

            var desktopEntry = new StringBuilder();
            desktopEntry.AppendFormat(
                "[Desktop Entry]\nName={0}\nExec={1} -batchmode -nographics %u\nType=Application\nTerminal=false\nMimeType=x-scheme-handler/{0}",
                name, location.ToString()
            );

            var addDesktopFile = new StringBuilder();
            addDesktopFile.AppendFormat(
                "echo '{0}' > ~/.local/share/applications/{1}.desktop", desktopEntry.ToString(), name
            );

            var mimeAppEntry = new StringBuilder();
            mimeAppEntry.AppendFormat("x-scheme-handler/{0}={0}.desktop", name);

            var updateMimeList = new StringBuilder();
            updateMimeList.AppendFormat(
                "echo {0} >> ~/.local/share/applications/mimeapps.list", mimeAppEntry.ToString()
            );

            var containsEntry = new StringBuilder();
            containsEntry.AppendFormat(
                "grep -Fx '{0}' ~/.local/share/applications/mimeapps.list", mimeAppEntry.ToString()
            );

            addDesktopFile.ToString().Bash();
            var hasEntry = containsEntry.ToString().Bash();
            if (String.IsNullOrEmpty(hasEntry))
            {
                updateMimeList.ToString().Bash();
                "update-desktop-database ~/.local/share/applications".Bash();
            }
        }

        private static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return result;
        }
#endif

        public static void HandleTempTx(string name)
        {
#if UNITY_STANDALONE
            if (Application.isBatchMode)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                SSOStore store = new SSOStoreLocalStorage();
                store.Load();

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i].StartsWith(name))
                    {
                        store.TmpTx = args[i];
                        store.Save();
                        break;
                    }
                }
                Application.Quit();
            }
#endif
        }
    }
}