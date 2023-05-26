using System.Drawing;
using System.Drawing.Printing;
using System.Management;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Graph;
using System.Net.Http.Json;


// Client-Side Console Application


class Program
{
    private static readonly HttpClient httpClient = new HttpClient();

    static async Task Main(string[] args)
    {
        string deviceInstancePath = "USBPRINT\\HPHP_COLORLASERJET_M255-M256\\7&AFEBC65&0&USB001";
        string printerName = GetPrinterName(deviceInstancePath);
        printerName = "HP ColorLaserJet M255-M256";

        Console.WriteLine("Client application started. Listening for print requests.");

        while (true)
        {
            try
            {
                var response = await httpClient.GetAsync("http://localhost:5039/");
                if (response.IsSuccessStatusCode)
                {
                    var printRequest = await response.Content.ReadFromJsonAsync<PrintRequest>();

                    // Process the print request and send it to the printer
                    PrintLabel(printRequest.LabelText, printerName);

                    Console.WriteLine("Print request processed successfully.");
                }
                else
                {
                    Console.WriteLine("Failed to retrieve print request.");
                }

                // Wait for a certain time before checking for new print requests
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private static void PrintLabel(string labelText, string printerName)
    {
        if (printerName != null)
        {
            // Create your print document and set up the content

            // Use the printerName to send the print request
            PrintDocument printDoc = new PrintDocument();
            printDoc.PrinterSettings.PrinterName = printerName;
            printDoc.PrintPage += (sender, e) =>
            {
                // Add your printing logic here
                // This event will be triggered when the document is being printed

                // For example, you can add text to be printed
                e.Graphics.DrawString("Wednesday", new System.Drawing.Font("Arial", 24), Brushes.Black, 10, 10);
                e.Graphics.DrawString("01/03/2023", new System.Drawing.Font("Arial", 24), Brushes.Black, 10, 54);
                e.Graphics.DrawString("SITE 113 Blaco Hill", new System.Drawing.Font("Arial", 20), Brushes.Black, 10, 98);
                e.Graphics.DrawString("FIELD 01 Sowerbutts", new System.Drawing.Font("Arial", 20), Brushes.Black, 10, 138);
                e.Graphics.DrawString("SIZE 20 - 30 Box No 1", new System.Drawing.Font("Arial", 20), Brushes.Black, 10, 178);
                e.Graphics.DrawString("Line Carrot Line", new System.Drawing.Font("Arial", 20), Brushes.Black, 10, 213);
                e.Graphics.DrawString("Ticket No. 89225 / Box No. 628286", new System.Drawing.Font("Arial", 16), Brushes.Black, 10, 253);

                //BarcodeWriter<Bitmap> barcodeWriter = new BarcodeWriter<Bitmap>
                //{
                //    Format = BarcodeFormat.CODE_128,  // or any other format you need
                //    Options = new EncodingOptions
                //    {
                //        Height = 150,
                //        Width = 200
                //    }
                //};

                //var barcodeBitmap = barcodeWriter.Write("628286");

                var barcodeWriter = new ZXing.BarcodeWriterPixelData
                {
                    Format = ZXing.BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 150,
                        Width = 200
                    }
                };
                var barcodeBitmap = PixelDataToBitmap(barcodeWriter.Write("628286"));

                e.Graphics.DrawImage(barcodeBitmap, 10, 289);

            };

            printDoc.Print();
        }
        else
        {
            // Unable to find the printer name for the given device instance path
            // Handle the error accordingly
        }
    }

    private static Bitmap PixelDataToBitmap(PixelData pixelData)
    {
        var bitmap = new Bitmap(pixelData.Width, pixelData.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, pixelData.Width, pixelData.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmapData.Scan0, pixelData.Pixels.Length);

        bitmap.UnlockBits(bitmapData);

        return bitmap;
    }

    private static string GetPrinterName(string path)
    {
        var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Printer");
        foreach (ManagementObject printer in searcher.Get())
        {
            if (printer["DeviceID"].ToString() == path)
            {
                return printer["Name"].ToString();
            }
        }
        return null;
    }
}

public class PrintRequest
{
    public string LabelText { get; set; }
    public string PrinterName { get; set; }
}
