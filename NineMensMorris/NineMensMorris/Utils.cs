using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace NineMensMorris
{
    public static class Utils
    {
        public enum GameModeEnum
        {
            WaitOnOther,
            SetUp,
            Mark,
            Move,
            Remove,
            Remove2,
            Jump,
            End
        }
        public enum RemoteMode
        {
            Host,
            Client,
            Computer
        }

        public static object DeserializeXml(Type returnType, string xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(returnType);

            using (TextReader reader = new StringReader(xmlString))
            {
                return serializer.Deserialize(reader);
            }
        }
        public static string SerializeXml(object objectToSerialize, Type serializationType)
        {
            var utf8NoBom = new UTF8Encoding(false);
            string ret = null;
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            var memoryStream = new MemoryStream();
            var xs = new XmlSerializer(serializationType);
            using (var xmlTextWriter = XmlWriter.Create(memoryStream, new XmlWriterSettings() { Encoding = utf8NoBom }))
            {
                xs.Serialize(xmlTextWriter, objectToSerialize, ns);
                ret = Encoding.UTF8.GetString(memoryStream.ToArray());
            }
            return ret;
        }
        public static string MessageXML(string message)
        {
            string ret = null;
            ActionListMessage.ActionList actionListXML = new ActionListMessage.ActionList();
            actionListXML.Header = new ActionListMessage.ActionListHeader() { HostName = System.Net.Dns.GetHostName(), IsAbort = false, Message = message };
            ret = Utils.SerializeXml(actionListXML, typeof(ActionListMessage.ActionList));
            return ret;
        }
        public static DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

    }
}
