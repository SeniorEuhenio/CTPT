using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CoreTeamPrivacyTool
{
    public partial class FormAAT : Form
    {
        private static string Version = "0.2";

        public FormAAT()
        {
            InitializeComponent();

            this.Text += $" ({Version})";
        }

        internal const string Inputkey = "{EC40FDD0-115F-441F-8960-1BF3C7F848D5}";

        private static RijndaelManaged NewRijndaelManaged(string passphrase)
        {
            if (passphrase == null)
                throw new PassphraseToShortException();

            var passphraseBytes = Encoding.ASCII.GetBytes(passphrase);
            var key = new Rfc2898DeriveBytes(Inputkey, passphraseBytes);

            var aesAlg = new RijndaelManaged();
            aesAlg.Key = key.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = key.GetBytes(aesAlg.BlockSize / 8);

            return aesAlg;
        }

        public static string EncryptRijndael(string text, string passphrase)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            var aesAlg = NewRijndaelManaged(passphrase);

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(text);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }

        public static string DecryptRijndael(string cipherText, string passphrase)
        {
            if (string.IsNullOrEmpty(cipherText))
                return "";

            string text;

            var aesAlg = NewRijndaelManaged(passphrase);
            var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            var cipher = Convert.FromBase64String(cipherText);

            using (var msDecrypt = new MemoryStream(cipher))
            {
                using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        text = srDecrypt.ReadToEnd();
                    }
                }
            }
            return text;
        }

        private void buttonEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxEncrypted.Text = EncryptRijndael(textBoxDecrypted.Text, textBoxSecret.Text);
                textBoxDecrypted.Text = "";
            }
            catch (PassphraseToShortException)
            {
                MessageBox.Show(this, "Secret needs to be 8 chars at least", "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                textBoxDecrypted.Text = DecryptRijndael(textBoxEncrypted.Text, textBoxSecret.Text);
                textBoxEncrypted.Text = "";
            }
            catch (PassphraseToShortException)
            {
                MessageBox.Show(this, "Secret needs to be 8 chars at least", "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Whoopsie", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonClearE_Click(object sender, EventArgs e)
        {
            textBoxEncrypted.Text = "";
        }

        private void buttonCopyE_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxEncrypted.Text, TextDataFormat.UnicodeText);
        }

        private void buttonPasteE_Click(object sender, EventArgs e)
        {
            textBoxEncrypted.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBoxDecrypted.Text, TextDataFormat.UnicodeText);
        }

        private void buttonPaste_Click(object sender, EventArgs e)
        {
            textBoxDecrypted.Text = Clipboard.GetText(TextDataFormat.UnicodeText);
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            textBoxDecrypted.Text = "";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var form = new FormAbout()
            {
                ShowInTaskbar = false,
                StartPosition = FormStartPosition.CenterParent
            })
            {
                form.ShowDialog(this);
            }
        }
    }
}
