namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) &&
                    Directory.Exists(txtLeftDir.Text))
                {
                    dlg.SelectedPath = txtLeftDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtLeftDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwLeftDir, dlg.SelectedPath);
                }
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            using (var dlg = new FolderBrowserDialog())
            {
                dlg.Description = "폴더를 선택하세요.";
                // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                if (!string.IsNullOrWhiteSpace(txtRightDir.Text) &&
                    Directory.Exists(txtRightDir.Text))
                {
                    dlg.SelectedPath = txtRightDir.Text;
                }
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    txtRightDir.Text = dlg.SelectedPath;
                    PopulateListView(lvwLeftDir, txtLeftDir.Text);
                    PopulateListView(lvwrightDir, dlg.SelectedPath);
                }
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {
            {
                var selected = lvwLeftDir.SelectedItems;

                foreach (ListViewItem item in selected)
                {
                    string name = item.Text;

                    string srcPath = Path.Combine(txtLeftDir.Text, name);
                    string destPath = Path.Combine(txtRightDir.Text, name);

                    // 파일 없으면 패스
                    if (!File.Exists(srcPath))
                        continue;

                    // 🔥 덮어쓰기 확인
                    if (File.Exists(destPath))
                    {
                        DateTime srcTime = File.GetLastWriteTime(srcPath);
                        DateTime destTime = File.GetLastWriteTime(destPath);
                        if (srcTime > destTime)
                        {
                            // 아무것도 안 하고 바로 진행
                        }
                        else
                        {
                            DialogResult result = MessageBox.Show(
                                    $"이미 동일한 이름의 파일이 있습니다.\n\n" +
                                    $"원본 파일 (Left)\n{srcPath}\n수정일: {srcTime}\n\n" +
                                    $"대상 파일 (Right)\n{destPath}\n수정일: {destTime}\n\n" +
                                    $"덮어쓰시겠습니까?",
                                    "덮어쓰기 확인",

                            MessageBoxButtons.YesNo
                        );

                            if (result == DialogResult.No)
                                continue;
                        }

                        // 🔥 복사
                        File.Copy(srcPath, destPath, true);
                    }

                    // 🔥 갱신 (중요)
                    PopulateListView(lvwLeftDir, txtLeftDir.Text);
                    PopulateListView(lvwrightDir, txtRightDir.Text);
                }

            }
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            var selected = lvwrightDir.SelectedItems;

            foreach (ListViewItem item in selected)
            {
                string name = item.Text;

                string srcPath = Path.Combine(txtRightDir.Text, name);
                string destPath = Path.Combine(txtLeftDir.Text, name);

                if (!File.Exists(srcPath))
                    continue;

                if (File.Exists(destPath))
                {
                    DateTime srcTime = File.GetLastWriteTime(srcPath);
                    DateTime destTime = File.GetLastWriteTime(destPath);
                    if (srcTime > destTime)
                    {
                        // 아무것도 안 하고 바로 진행
                    }
                    else
                    {

                        DialogResult result = MessageBox.Show(
                    $"이미 동일한 이름의 파일이 있습니다.\n\n" +
                    $"원본 파일 (Right)\n{srcPath}\n수정일: {srcTime}\n\n" +
                    $"대상 파일 (Left)\n{destPath}\n수정일: {destTime}\n\n" +
                    $"덮어쓰시겠습니까?",
                    "덮어쓰기 확인",
                        MessageBoxButtons.YesNo
                    );

                        if (result == DialogResult.No)
                            continue;
                    }

                    File.Copy(srcPath, destPath, true);
                }

                PopulateListView(lvwLeftDir, txtLeftDir.Text);
                PopulateListView(lvwrightDir, txtRightDir.Text);
            }
        }

        private void lvwLeftDir_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void lvwrightDir_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        private void PopulateListView(ListView lv, string folderPath)
        {
            lv.BeginUpdate();
            lv.Items.Clear();

            try
            {
                // 폴더 추가
                var dirs = Directory.EnumerateDirectories(folderPath)
                    .Select(p => new DirectoryInfo(p))
                    .OrderBy(d => d.Name);

                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add(d.LastWriteTime.ToString("g"));
                    
                   
                    lv.Items.Add(item);



                }

                // 파일 추가
                var files = Directory.EnumerateFiles(folderPath)
                    .Select(p => new FileInfo(p))
                    .OrderBy(f => f.Name);

                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트");
                    item.SubItems.Add(f.LastWriteTime.ToString("g"));
                    FileInfo rf = null;

                    string otherPath = (lv == lvwLeftDir) ? txtRightDir.Text : txtLeftDir.Text;

                    if (Directory.Exists(otherPath))
                    {
                        var otherFiles = Directory.GetFiles(otherPath)
                            .Select(p => new FileInfo(p));

                        foreach (var of in otherFiles)
                        {
                            if (of.Name == f.Name)
                            {
                                rf = of;
                                break;
                            }
                        }
                    }

                    // 상태 결정 및 색상 적용 (PDF 그대로)
                    if (rf != null)
                    {
                        if (f.LastWriteTime == rf.LastWriteTime)
                        {
                            item.ForeColor = Color.Black;
                        }
                        else
                        {
                            item.ForeColor = (lv == lvwLeftDir) ? Color.Gray : Color.Red;
                        }
                    }
                    else
                    {
                        item.ForeColor = Color.Purple;
                    }


                    lv.Items.Add(item);
                }

                // 컬럼 자동 크기
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.AutoResizeColumn(i, ColumnHeaderAutoResizeStyle.ColumnContent);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("오류: " + ex.Message);
            }
            finally
            {
                lv.EndUpdate();
            }

        }

    }

}
