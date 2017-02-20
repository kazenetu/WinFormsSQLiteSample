using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;

namespace SQLiteSample.App_Code.Database
{
    /// <summary>
    /// SQLiteラッパークラス
    /// </summary>
    public class SQLiteDB : IDisposable
    {
        #region プライベートフィールド

        /// <summary>
        /// コネクションインスタンス
        /// </summary>
        private SQLiteConnection conn = null;

        /// <summary>
        /// トランザクションインスタンス
        /// </summary>
        private SQLiteTransaction tran = null;

        /// <summary>
        /// パラメータ
        /// </summary>
        private Dictionary<string, object> param;

        /// <summary>
        /// トランザクションが開いているか否か
        /// </summary>
        private bool isTran = false;

        #endregion

        #region パブリックメソッド

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="connectionString">接続文字列</param>
        public SQLiteDB(string connectionString)
        {
            this.conn = this.getConnection(connectionString);
            this.conn.Open();
            this.tran = this.conn.BeginTransaction();
            this.isTran = true;

            this.param = new Dictionary<string, object>();
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="server">Serverインスタンス</param>
        /// <remarks>デフォルトの設定を利用する</remarks>
        public SQLiteDB()
        {
            this.conn = this.getConnection("Data Source=" + System.IO.Directory.GetCurrentDirectory() + @"\test.db");
            this.conn.Open();
            this.tran = this.conn.BeginTransaction();
            this.isTran = true;

            this.param = new Dictionary<string, object>();
        }

        /// <summary>
        /// パラメータを追加
        /// </summary>
        /// <param name="key">キー</param>
        /// <param name="value">値</param>
        public void AddParam(string key, object value)
        {
            this.param.Add(key, value);
        }

        /// <summary>
        /// パラメータをクリア
        /// </summary>
        public void ClearParam()
        {
            this.param.Clear();
        }

        /// <summary>
        /// SQL実行（登録・更新・削除）
        /// </summary>
        /// <param name="sql">SQLステートメント</param>
        /// <returns>処理件数</returns>
        public int ExecuteNonQuery(string sql)
        {
            using (SQLiteCommand command = conn.CreateCommand())
            {
                command.CommandText = sql;

                foreach (var key in this.param.Keys)
                {
                    command.Parameters.AddWithValue(key, this.param[key]);
                }

                return command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// 検索SQL実行
        /// </summary>
        /// <param name="sql">SQLステートメント</param>
        /// <returns>検索結果</returns>
        public DataTable Fill(string sql)
        {
            using (SQLiteCommand command = conn.CreateCommand())
            {
                command.CommandText = sql;

                foreach (var key in this.param.Keys)
                {
                    command.Parameters.AddWithValue(key, this.param[key]);
                }

                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    //スキーマ取得
                    var result = this.GetShcema(reader);

                    while (reader.Read())
                    {
                        var addRow = result.NewRow();

                        foreach (DataColumn col in result.Columns)
                        {
                            addRow[col.ColumnName] = reader[col.ColumnName];
                        }

                        result.Rows.Add(addRow);
                    }

                    return result;
                }
            }
        }

        /// <summary>
        /// コミット
        /// </summary>
        public void Commit()
        {
            this.tran.Commit();
            this.isTran = false;
        }

        /// <summary>
        /// ロールバック
        /// </summary>
        public void Rollback()
        {
            this.tran.Rollback();
            this.isTran = false;
        }

        /// <summary>
        /// 解放処理
        /// </summary>
        public void Dispose()
        {
            if (this.isTran)
            {
                this.tran.Rollback();
            }
            this.tran.Dispose();
            this.conn.Close();
            this.conn.Dispose();
        }

        #endregion

        #region プライベートメソッド

        /// <summary>
        /// コネクション生成
        /// </summary>
        /// <param name="connectionString">接続文字列</param>
        /// <returns>コネクションインスタンス</returns>
        private SQLiteConnection getConnection(string connectionString)
        {
            return new SQLiteConnection(connectionString);
        }

        /// <summary>
        /// スキーマ取得
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private DataTable GetShcema(SQLiteDataReader reader)
        {
            var result = new DataTable();

            for (var i = 0; i < reader.FieldCount; i++)
            {
                result.Columns.Add(new DataColumn(reader.GetName(i)));
            }

            return result;
        }
        #endregion
    }
}