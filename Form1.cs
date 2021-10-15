using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Azure.Storage.Blobs;
using Azure.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace AzureUploadDownloader
{
    public partial class Form1 : Form
    {
        private string storageConnectionString = "";
        private CloudBlobClient cloudBlobClient;
        private CloudBlobContainer cloudBlobContainer;
        private bool connected = false;
        public Form1()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private async void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "")
            {
                MessageBox.Show("Missing Connection String");
            }
            else if (textBox2.Text == "")
            {
                MessageBox.Show("Missin Container Blob Name");
            }
            else
            {

                CloudStorageAccount storageAccount;
                if (CloudStorageAccount.TryParse(textBox1.Text, out storageAccount))
                {
                    // If the connection string is valid, proceed with operations against Blob
                    // storage here.
                    // ADD OTHER OPERATIONS HERE
                    cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create a container called 'quickstartblobs' and 
                    // append a GUID value to it to make the name unique.
                    cloudBlobContainer =
                        cloudBlobClient.GetContainerReference(textBox2.Text);
                    try
                    {
                        richTextBox1.Text += "Trying to create containeer:" + textBox2.Text + Environment.NewLine;
                        await cloudBlobContainer.CreateAsync();
                        BlobContainerPermissions permissions = new BlobContainerPermissions
                        {
                            PublicAccess = BlobContainerPublicAccessType.Off
                        };
                        await cloudBlobContainer.SetPermissionsAsync(permissions);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("409"))
                        {
                            richTextBox1.Text += "Container already exist" + Environment.NewLine;
                        }
                        else
                        {
                            richTextBox1.Text += ex.Message + Environment.NewLine;
                        }
                    }
                    finally
                    {
                        listBox1.Items.Clear();
                        BlobContinuationToken blobContinuationToken = null;
                        do
                        {
                            var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                            // Get the value of the continuation token returned by the listing call.
                            blobContinuationToken = results.ContinuationToken;
                            foreach (IListBlobItem item in results.Results)
                            {
                                listBox1.Items.Add(item.Uri);
                            }
                        } while (blobContinuationToken != null); // Loop while the continuation token is not null.

                        richTextBox1.Text += "connected" + Environment.NewLine;
                        connected = true;
                    }
                }
                else
                {
                    // Otherwise, let the user know that they need to define the environment variable.
                    MessageBox.Show(
                        "A connection string has not been defined in the system environment variables. " +
                        "Add an environment variable named 'AZURE_STORAGE_CONNECTION_STRING' with your storage " +
                        "connection string as a value.");
                }
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            //openFileDialog1.Filter = "zip";
            if (!connected)
            {
                MessageBox.Show("Please connect before uploading");
            }
            else if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                listBox1.Items.Clear();
                CloudBlockBlob  cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(openFileDialog1.SafeFileName);
                await cloudBlockBlob.UploadFromFileAsync(openFileDialog1.FileName);
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloudBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    // Get the value of the continuation token returned by the listing call.
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        listBox1.Items.Add(item.Uri);
                    }
                } while (blobContinuationToken != null); // Loop while the continuation token is not null.

                richTextBox1.Text += "connected" + Environment.NewLine;
                connected = true;
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                if (listBox1.SelectedItem != null)
                {
                    if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                    {
                        foreach (var item in listBox1.SelectedItems)
                        {
                            var file = item.ToString();

                            //CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(file);

                            var filename = file.Split('/')[file.Split('/').Length - 1];
                            richTextBox1.Text += "downloading : " + filename + Environment.NewLine;
                            CloudBlockBlob cloudBlockBlob = cloudBlobContainer.GetBlockBlobReference(filename);
                            await cloudBlockBlob.DownloadToFileAsync(folderBrowserDialog1.SelectedPath + "\\" + filename, FileMode.Create);
                            richTextBox1.Text += file + " Downloaded" + Environment.NewLine;
                        }
                    }//
                }
            }
        }
    }
}
