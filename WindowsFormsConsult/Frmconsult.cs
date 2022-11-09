using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace WindowsFormsConsult
{
    public partial class Frmconsult : Form
    {
        public Frmconsult()
        {
            InitializeComponent();
            datepkTo.MinDate = datepkFrm.Value;
        }
        RehablitationEntities rdb = new RehablitationEntities();
        
        private void btn新增_Click(object sender, EventArgs e)
        {
            int ptid = 0;                         //存病人id用
             //todo 新增、修改的防呆
            Consultation consu = new Consultation   //會談的文字內容
            {
                //todo patientid 還沒抓，要看去哪抓
                Date = datePk.Value.ToString("yyyy/MM/dd"),
                //PatinetId =ptid,    
                Summary = txt內容.Text,
                Assessment = txt評估.Text,
                Result = txt成效.Text,
            };
            rdb.Consultation.Add(consu);     
            rdb.SaveChanges();
            
            int fconsultid = consu.fConsultId;                        //把本次會談的fcounsultid存下來給下面checkbox用

           CheckBox[] chb = new CheckBox[10];              //做10個checkbox並用迴圈方式篩選是否有打勾，
            chb = new CheckBox[10] {ch1,ch2,ch3,ch4,ch5,ch6,ch7,ch8,ch9,ch10};

           for(int i = 1; i <= 10; i++)
            {
                if (chb[i-1].CheckState == CheckState.Checked)
                {
                    rdb.CounsultTypeRecord.Add(new CounsultTypeRecord
                    {
                        TypeNameId = i,                                                                             //有打勾的話就把相對應的數字新增到[CounsultTypeRecord]table中
                        fConsultId = fconsultid,
                    });
                };
            }         
            rdb.SaveChanges();
            MessageBox.Show("資料新增成功");

        }

        private void btn修改_Click(object sender, EventArgs e)
        {
            //todo patientid 查詢也需要
            var q = rdb.Consultation.First(ca => ca.fConsultId == resultid);
            //q.PatinetId = ;
            q.Date = datePk.Value.ToString("yyyy/MM/dd");
            q.Summary = txt內容.Text;
            q.Assessment = txt評估.Text;
            q.Result = txt成效.Text;

            CheckBox[] chb = new CheckBox[10];                                                                   //做10個checkbox並用迴圈方式篩選是否有打勾，
            chb = new CheckBox[10] { ch1, ch2, ch3, ch4, ch5, ch6, ch7, ch8, ch9, ch10 };
           
            var tp = from tt in rdb.CounsultTypeRecord     //刪除所有該流水號的checkbox資料
                     where tt.fConsultId == resultid
                     select tt;
            rdb.CounsultTypeRecord.RemoveRange(tp);

            for (int i = 1; i <= 10; i++)
            {
                if (chb[i - 1].CheckState == CheckState.Checked)
                {
                    rdb.CounsultTypeRecord.Add(new CounsultTypeRecord
                    {
                        TypeNameId = i,                                                                             //有打勾的話就把相對應的數字以"新增"方式加到[CounsultTypeRecord]table中
                        fConsultId = resultid,                                                                    //因為上面已經刪掉所有的資料了，所以用新增方式
                    });
                };
            }            
            rdb.SaveChanges(); MessageBox.Show("資料修改成功");
        }


        private void btn刪除_Click(object sender, EventArgs e)
        {

            string deleDate = datePk.Value.ToString("yyyy/MM/dd");
            DialogResult Result = MessageBox.Show("確定刪除此筆資料?", "資料刪除", MessageBoxButtons.OKCancel);

            if (Result == DialogResult.OK)
            {
                var q = rdb.Consultation.First(ca => ca.Date == deleDate);   //刪除consultation
                int fconsultid = q.fConsultId;
                rdb.Consultation.Remove(q);

                var tp = from tt in rdb.CounsultTypeRecord     //刪除type
                         where tt.fConsultId == fconsultid
                         select tt;
                rdb.CounsultTypeRecord.RemoveRange(tp);


                rdb.SaveChanges();
                MessageBox.Show("資料刪除成功", "資料刪除");
            }
            
        }
        

        List<int> pKeys = null;
        
        private void btn查詢_Click(object sender, EventArgs e)
        {
            //datepkTo.MinDate = datepkFrm.Value;
            string timepkfrm = datepkFrm.Value.ToString("yyyy/MM/dd");
            string timepkto = datepkTo.Value.ToString("yyyy/MM/dd");

            listBox1.Items.Clear();
            pKeys = new List<int>();
            var eee = (from ee in rdb.Consultation
                       where ee.Date.CompareTo(timepkfrm) >=0 && ee.Date.CompareTo(timepkto) <= 0
                       orderby ee.Date  descending
                       select ee).ToList();
                       
            foreach (var item in eee) //應該要用result 取代 eee
            {
                listBox1.Items.Add(item.Date);   //listbox[0] 的東西 其fconsultid存在 pkeys[0]
                pKeys.Add(item.fConsultId);
            }      
        }

        int resultid = 0;
        private void listboxClick(object sender, EventArgs e)
        {

            if (listBox1.SelectedIndex < 0)
                return;
             resultid = pKeys[listBox1.SelectedIndex];                 
            //MessageBox.Show(pKeys[result].ToString());  //測試用，點選listbox出現其index
            
            Consultation consu = new Consultation();
            var q = (from cc in rdb.Consultation
                     where cc.fConsultId == resultid
                     select cc).FirstOrDefault();

            datePk.Text = string.IsNullOrEmpty(q?.Date) ? "" : q.Date;
            txt內容.Text = string.IsNullOrEmpty(q?.Summary)?"":q.Summary.Trim();
            txt評估.Text = string.IsNullOrEmpty(q?.Assessment) ? "" : q.Assessment.Trim();
            txt成效.Text = string.IsNullOrEmpty(q?.Result) ? "" : q.Result.Trim();

            CheckBox[] chb = new CheckBox[10];                                                                   //做10個checkbox並用迴圈方式篩選是否有打勾，
            chb = new CheckBox[10] { ch1, ch2, ch3, ch4, ch5, ch6, ch7, ch8, ch9, ch10 };
            foreach(var ch in chb)
            {
                ch.Checked = false;
            }

            CounsultTypeRecord cnsltTypRcd = new CounsultTypeRecord();
            var typename = (from tyn in rdb.CounsultTypeRecord
                           where tyn.fConsultId == resultid
                           select tyn.TypeNameId).ToList();

           
            List<object> typid = new List<object>();
            foreach(var typeid in typename)
            {
                //typid.Add(typeid);
                chb[Convert.ToInt32(typeid) - 1].Checked = true;

            }
        }
         //測試用 yeah 
        private void datepkFrm_ValueChanged(object sender, EventArgs e)
        {
            datepkTo.MinDate = datepkFrm.Value;
        }
    }
}
