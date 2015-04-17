using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.IO;
using System.Text;

namespace Phoneword_Droid
{
    /// <summary>
    /// Main activity of Phoneword application.
    /// </summary>
    [Activity(Label = "Phoneword", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {

        static readonly List<string> phoneNumbers = new List<string>();
        static bool startedOnce = false;
        static string filePath;
        const string FILE_NAME = "phone_numbers.txt";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            EditText phoneNumberText = FindViewById<EditText>(Resource.Id.PhoneNumberText);
            Button translateButton = FindViewById<Button>(Resource.Id.TranslateButton);
            Button callButton = FindViewById<Button>(Resource.Id.CallButton);
            Button callHistoryButton = FindViewById<Button>(Resource.Id.CallHistoryButton);

            if (!startedOnce) {
                var documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
                filePath = Path.Combine (documentsPath, FILE_NAME);
                if (!File.Exists(filePath)) {
                    File.Create(filePath).Close();
                }
                string fileContent = File.ReadAllText (filePath);
                Console.WriteLine("PreCount: " + phoneNumbers.Count);
                if (fileContent.Length > 0) {
                    phoneNumbers.AddRange(fileContent.TrimEnd().Split(new char[] {'\n'}));
                }
                Console.WriteLine("Text: " + fileContent.TrimEnd());
                Console.WriteLine("Text length: " + fileContent.Length);
                startedOnce = true;
            }


            Console.WriteLine("Count: " + phoneNumbers.Count);

            if (phoneNumbers.Count > 0) {
                callHistoryButton.Enabled = true;
            }

            //Disable the "Call" button
            callButton.Enabled = false;

            //Add code to translate number
            string translatedNumber = string.Empty;

            translateButton.Click += (object sender, EventArgs e) => 
            {
                //Translate user's alphanumeric number to numeric
                translatedNumber = Core.PhonewordTranslator.ToNumber(phoneNumberText.Text);
                if (string.IsNullOrWhiteSpace(translatedNumber)) {
                    callButton.Text = "Call";
                    callButton.Enabled = false;
                } else {
                    callButton.Text = "Call " + translatedNumber;
                    callButton.Enabled = true;
                }
            };

            callButton.Click += (object sender, EventArgs e) => 
            {
                
                //On "Call" button click try to dial phone number
                var callDialog = new AlertDialog.Builder(this);
                callDialog.SetMessage("Call " + translatedNumber + "?");
                callDialog.SetNeutralButton("Call", delegate {
                    // add dialed number to list of called numbers.
                    phoneNumbers.Add(translatedNumber);
                    //Write new number to file
                    File.AppendAllText(filePath, translatedNumber + "\n");
                    // enable the Call History button
                    callHistoryButton.Enabled = true;
                    //Create intent to dial phone
                    var callIntent = new Intent(Intent.ActionCall);
                    callIntent.SetData(Android.Net.Uri.Parse("tel:" + translatedNumber));
                    StartActivity(callIntent);
                });
                callDialog.SetNegativeButton("Cancel", delegate { });

                //Show the alert dialog to the user and wait for response
                callDialog.Show();
            };


            callHistoryButton.Click += (sender, e) => 
            {
                var intent = new Intent(this, typeof(CallHistoryActivity));
                intent.PutStringArrayListExtra("phone_numbers", phoneNumbers);
                StartActivity(intent);
            };

        }
    }
}


