using System.Windows.Forms;

namespace SuvirinimaiApp
{
    class Msg
    {
        public static void ErrorMsg(string msg)
        {
            MessageBox.Show(msg,
                   SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Error);
        }

        public static void InformationMsg(string msg)
        {
            MessageBox.Show(msg,
                   SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information);
        }

        public static void ExclamationMsg(string msg)
        {
            MessageBox.Show(msg,
                   SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Exclamation);
        }

        public static void QuestionMsg(string msg)
        {
            MessageBox.Show(msg,
                   SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Question);
        }

        public static void WarningMsg(string msg)
        {
            MessageBox.Show(msg,
                   SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Warning);
        }

        public static DialogResult YesNoCancelQuestionMsg(string msg)
        {
            return MessageBox.Show(
                                     msg,
                                     SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                                     MessageBoxButtons.YesNoCancel,
                                     MessageBoxIcon.Question,
                                     MessageBoxDefaultButton.Button3
                                     );
        }

        public static DialogResult YesNoQuestionMsg(string msg)
        {
            return MessageBox.Show(
                                     msg,
                                     SuvirinimaiApp.Properties.Messages.MsgBoxTitle,
                                     MessageBoxButtons.YesNo,
                                     MessageBoxIcon.Question,
                                     MessageBoxDefaultButton.Button2
                                     );
        }
    }
}
