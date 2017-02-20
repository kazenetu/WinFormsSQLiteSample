using SQLiteSample.App_Code.Database;
using System;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsSQLite
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var db = new SQLiteDB())
            {
                //SQL発行
                var sql = new StringBuilder();
                sql.Append("SELECT");
                sql.Append("  USER_ID ");
                sql.Append("  ,NAME ");
                sql.Append("  ,PASSWORD ");
                sql.Append("FROM MT_USER ");

                var result = db.Fill(sql.ToString());
                if (result.Rows.Count > 0)
                {
                    this.dataGridView1.DataSource = result;
                }else
                {
                    this.dataGridView1.DataSource = null;
                }

            }
        }
    }
}
