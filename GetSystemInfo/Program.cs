using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Management;
using System.IO;
using System.Linq;
using System.Globalization;

namespace GetSystemInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(getCPU());
            Console.WriteLine(getNetworkBandwith());
            Console.WriteLine(getHarddrive());
            Console.WriteLine(getOs());
            Console.WriteLine(getMemory());
        }

        public static string getNetworkBandwith()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(String.Format("Network Adapters:\n"));

            foreach (NetworkInterface netInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                // Only consider what is being used and avoid Loopback interface.
                if (netInterface.OperationalStatus == OperationalStatus.Up &&
                    netInterface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    strBuilder.Append(String.Format("Name: {0}; Speed: {1} MB/s\n", netInterface.Name, Math.Round(netInterface.Speed / (8.0 * 1024 * 1024))));
                }
            }

            return strBuilder.ToString();
        }

        // https://ourcodeworld.com/articles/read/294/how-to-retrieve-basic-and-advanced-hardware-and-software-information-gpu-hard-drive-processor-os-printers-in-winforms-with-c-sharp
        public static string getCPU()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(String.Format("CPU:\n"));

            ManagementObjectSearcher myProcessorObject = new ManagementObjectSearcher("select * from Win32_Processor");
            foreach (ManagementObject obj in myProcessorObject.Get())
            {
                strBuilder.Append(String.Format("Name: {0}\n", obj["Name"]));
                strBuilder.Append(String.Format("NumberOfCores: {0}\n", obj["NumberOfCores"]));
                strBuilder.Append(String.Format("NumberOfEnabledCore: {0}\n", obj["NumberOfEnabledCore"]));
                strBuilder.Append(String.Format("NumberOfLogicalProcessors: {0}\n", obj["NumberOfLogicalProcessors"]));
            }

            return strBuilder.ToString();
        }

        public static string getMemory()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(String.Format("Memory:\n"));

            ObjectQuery winQuery = new ObjectQuery("SELECT * FROM CIM_OperatingSystem");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(winQuery);

            foreach (ManagementObject item in searcher.Get())
            {
                strBuilder.Append(String.Format("TotalVisibleMemorySize: {0} GB\n", Math.Round((ulong)item["TotalVisibleMemorySize"] / Math.Pow(1024, 2), 2)));
                strBuilder.Append(String.Format("TotalVirtualMemorySize: {0} GB\n", Math.Round((ulong)item["TotalVirtualMemorySize"] / Math.Pow(1024, 2), 2)));
                strBuilder.Append(String.Format("FreeVirtualMemory: {0} GB\n", Math.Round((ulong)item["FreeVirtualMemory"] / Math.Pow(1024, 2), 2)));
                strBuilder.Append(String.Format("FreeSpaceInPagingFiles: {0} GB\n", Math.Round((ulong)item["FreeSpaceInPagingFiles"] / Math.Pow(1024, 2), 2)));
                strBuilder.Append(String.Format("FreePhysicalMemory: {0} GB\n", Math.Round((ulong)item["FreePhysicalMemory"] / Math.Pow(1024, 2), 2)));
            }

            return strBuilder.ToString();
        }

        public static string getHarddrive()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(String.Format("Disk:\n"));

            DriveInfo[] allDrives = DriveInfo.GetDrives();
            double gb = Math.Pow(1024, 3);

            foreach (DriveInfo d in allDrives)
            {
                if (d.IsReady == true)
                {
                    strBuilder.Append(String.Format("Drive {0}\n", d.Name));
                    strBuilder.Append(String.Format("File system: {0}\n", d.DriveFormat));
                    strBuilder.Append(String.Format("Available space to current user:{0, 6} GB\n", Math.Round(d.AvailableFreeSpace/ gb)));
                    strBuilder.Append(String.Format("Total available space:          {0, 6} GB\n", Math.Round(d.TotalFreeSpace/ gb)));
                    strBuilder.Append(String.Format("Total size of drive:            {0, 6} GB\n", Math.Round(d.TotalSize/ gb)));
                }
            }

            ///////////////////////////// SSD vs HDD?
            ManagementScope scope = new ManagementScope(@"\\.\root\microsoft\windows\storage");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM MSFT_PhysicalDisk");
            scope.Connect();
            searcher.Scope = scope;

            foreach (ManagementObject queryObj in searcher.Get())
            {
                switch (Convert.ToInt16(queryObj["MediaType"]))
                {
                    case 1:
                        strBuilder.Append("DiskType: Unspecified\n");
                        break;

                    case 3:
                        strBuilder.Append("DiskType: HDD\n");
                        break;

                    case 4:
                        strBuilder.Append("DiskType: SSD\n");
                        break;

                    case 5:
                        strBuilder.Append("DiskType: SCM\n");
                        break;

                    default:
                        strBuilder.Append("DiskType: Unspecified\n");
                        break;
                }
            }
            searcher.Dispose();

            return strBuilder.ToString();
        }

        public static string getOs()
        {
            StringBuilder strBuilder = new StringBuilder();
            strBuilder.Append(String.Format("OS:\n"));

            ManagementObjectSearcher myOperativeSystemObject = new ManagementObjectSearcher("select * from Win32_OperatingSystem");

            foreach (ManagementObject obj in myOperativeSystemObject.Get())
            {
                strBuilder.Append(String.Format("Caption: {0}\n", obj["Caption"]));
                strBuilder.Append(String.Format("Version: {0}\n", obj["Version"]));
                //strBuilder.Append(String.Format("CountryCode: {0} => {1}\n", obj["CountryCode"], ISO3166.FromPhoneCode((string)obj["CountryCode"]).Aggregate("", (acc, x) => acc + x.Name + "; ")));
                strBuilder.Append(String.Format("LastBootUpTime: {0}\n", ManagementDateTimeConverter.ToDateTime(obj["LastBootUpTime"].ToString()).ToString("dd/MM/yyyy HH:mm:ss")));
                strBuilder.Append(String.Format("OSArchitecture: {0}\n", obj["OSArchitecture"]));
                strBuilder.Append(String.Format("InstalledUICultureLanguage: {0}\n", CultureInfo.InstalledUICulture.EnglishName));
                strBuilder.Append(String.Format("CurrentUICulture: {0}\n", CultureInfo.CurrentUICulture.EnglishName));
            }

            return strBuilder.ToString();
        }
    }
}
