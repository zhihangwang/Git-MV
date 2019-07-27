using System;
using System.IO;
using System.Diagnostics;

namespace git_mv
{

    class Program
    {
        private static string oldDirectory = "";
        private static string newDirectory = "";
        static void Main(string[] args)
        {
            Console.WriteLine("Input Git Move Out Directory:");
            oldDirectory = Console.ReadLine().Trim();
            Console.WriteLine("Input Git Move In Directory:");
            DirectoryInfo oldDirectoryInfo = new DirectoryInfo(oldDirectory);
            newDirectory = Console.ReadLine().Trim();
            CreateDirectory(newDirectory);
            var gitRoute = GetGitRoute(oldDirectoryInfo);
            Console.WriteLine("Move Start......");
            GetMvDirectory(oldDirectoryInfo, gitRoute);
            Console.WriteLine("Move End......");
            Console.ReadLine();
        }

        private static void CreateDirectory(string directoryRoute)
        {
            if (!Directory.Exists(directoryRoute))
            {
                Directory.CreateDirectory(directoryRoute);
            }

        }

        private static string GetGitRoute(DirectoryInfo oldDirectory)
        {
            if (HaveGitNameDirectory(oldDirectory) || oldDirectory.Parent == null)
            {
                return oldDirectory.FullName;
            }
            return GetGitRoute(oldDirectory.Parent);
        }

        private static bool HaveGitNameDirectory(DirectoryInfo directory)
        {
            var directories = directory.GetDirectories();
            var have = false;
            foreach (var directorie in directories)
            {
                if (!have)
                {
                    have = directorie.Name.ToLower() == ".git";
                }
            }
            return have;
        }

        private static void GetMvDirectory(DirectoryInfo directoryInfo, string gitRoute)
        {
            var childrenDirectories = directoryInfo.GetDirectories();
            foreach (var childrenDirectorie in childrenDirectories)
            {
                if (childrenDirectorie.Name != "bin" && childrenDirectorie.Name != "obj")
                    GetMvDirectory(childrenDirectorie, gitRoute);
            }
            GitMvFile(directoryInfo, gitRoute);
        }

        private static void GitMvFile(DirectoryInfo directory, string gitRoute)
        {
            var files = directory.GetFiles();
            foreach (var file in files)
            {
                var fileRoute = file.FullName.Replace(file.Name, "", StringComparison.InvariantCultureIgnoreCase);
                var fileFullRoute = $"{fileRoute}{file.Name}";
                var newRoute = newDirectory + fileRoute.Replace(oldDirectory, "", StringComparison.InvariantCultureIgnoreCase);
                CreateDirectory(newRoute);
                var newFullRoute = $"{newRoute}{file.Name}";
                Cmd.RunCmd(gitRoute, $"git mv {fileFullRoute} {newFullRoute}");
            }
        }
    }


    class Cmd
    {
        private static string CmdPath = @"C:\Windows\System32\cmd.exe";
        /// <summary>
        /// 执行cmd命令 返回cmd窗口显示的信息
        /// 多命令请使用批处理命令连接符：
        /// <![CDATA[
        /// &:同时执行两个命令
        /// |:将上一个命令的输出,作为下一个命令的输入
        /// &&：当&&前的命令成功时,才执行&&后的命令
        /// ||：当||前的命令失败时,才执行||后的命令]]>
        /// </summary>
        /// <param name="cmd">执行的命令</param>
        public static string RunCmd(string gitRoute,string cmd)
        {
            cmd = $"cd {gitRoute}&" + cmd.Trim().TrimEnd('&') + "&exit";//说明：不管命令是否成功均执行exit命令，否则当调用ReadToEnd()方法时，会处于假死状态
            using (Process p = new Process())
            {
                p.StartInfo.FileName = CmdPath;
                p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;  //由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
              
                p.Start();//启动程序

                //向cmd窗口写入命令
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();//等待程序执行完退出进程
                p.Close();

                return output;
            }
        }
    }
}