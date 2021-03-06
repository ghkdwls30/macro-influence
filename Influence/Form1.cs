﻿using MoreLinq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Influence
{
    public partial class Form1 : Form
    {



        IWebDriver driver;
        Thread browserWorker;
        string configFilePath = Application.StartupPath + @"\Config\System_Config.txt";
        Dictionary<string, string> globalConfig = new Dictionary<string, string>();
        string userAgentListPath = Application.StartupPath + @"\Config\User_Agent_List.txt";
        List<string> userAgentList = new List<string>();
        private DataTable dt;
        //string userListPath = Application.StartupPath + @"\Config\User_List.txt";
        // 라이센스
        string license = System.IO.File.ReadAllLines(Application.StartupPath + @"\Config\License.txt")[0];
        // 라이센스키
        string LICENSE_KEY = "rg9gDHJtjfpuJ4FZ";
        // 모드
        string mode;

        private SqlUtil sqlUtil = new SqlUtil();

        public Form1()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            // 데이타그리드세팅
            dataGridView1.EditMode = DataGridViewEditMode.EditProgrammatically;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.Gold;
            dataGridView1.EnableHeadersVisualStyles = false;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.EnableResizing;
            dataGridView1.ColumnHeadersHeight = 25;
            dataGridView1.AllowUserToAddRows = false;


            //DataTable 생성
            dt = new DataTable();
            //dataGridView에 dataTable 연결
            dataGridView1.DataSource = dt;

            dt.Columns.Add("날짜", typeof(string));
            dt.Columns.Add("닉네임", typeof(string));
            dt.Columns.Add("키워드", typeof(string));
            dt.Columns.Add("전체작업량", typeof(int));
            dt.Columns.Add("현재작업량", typeof(int));

            //글로벌 세팅
            string[] line = System.IO.File.ReadAllLines(configFilePath);
            if (line.Length > 0)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    if (!line[i].StartsWith("#") && line[i].Trim().Length > 0)
                    {
                        string[] c = line[i].Split('=');
                        globalConfig.Add(c[0], c[1]);
                    }
                }
            }

            // 유저에이전트
            line = System.IO.File.ReadAllLines(userAgentListPath);
            if (line.Length > 0)
            {
                for (int i = 0; i < line.Length; i++)
                {
                    userAgentList.Add(line[i]);
                }
            }

            // 라이센스복호화
            if (!"SHOWMETHEMONEY".Equals(license))
            {
                license = AESDecrypt128(license, LICENSE_KEY);
            }

            // 모드키워드 하단
            mode = getProperty("system.mode");

            if ("N".Equals(mode))
            {
                label5.Text = "닉네임 모드";
            }
        }

        private IWebDriver MakeDriver()
        {
            return MakeDriver(false, "Mozilla/5.0 (iPad; CPU OS 6_0 like Mac OS X) AppleWebKit/536.26 (KHTML, like Gecko) Version/6.0 Mobile/10A5355d Safari/8536.25");
        }
        private IWebDriver MakeDriver(bool isHide, string userAgent)
        {
            ChromeOptions cOptions = new ChromeOptions();
            cOptions.AddArguments("disable-infobars");
            cOptions.AddArguments("--js-flags=--expose-gc");
            cOptions.AddArguments("--enable-precise-memory-info");
            cOptions.AddArguments("--disable-popup-blocking");
            cOptions.AddArguments("--disable-default-apps");
            cOptions.AddArguments("--window-size=360,900");
            cOptions.AddArguments("--incognito");

            if (isHide)
            {
                cOptions.AddArguments("headless");
            }
            

            ChromeDriverService chromeDriverService = ChromeDriverService.CreateDefaultService();
            chromeDriverService.HideCommandPromptWindow = true;

                
            
            cOptions.AddArgument("--user-agent=" + userAgent);
            


            // 셀레니움실행
            IWebDriver driver = new ChromeDriver(chromeDriverService, cOptions);
            //driver.Manage().Window.Maximize();
            //driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(60);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(60);

            return driver;
        }


        private bool ScrollByTry(int c, By by, int maxLoop)
        {
            // 스크롤 다운icon-sprite icon-gender-f
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            IWebElement element = null;
            while (true)
            {
                maxLoop--;
                js.ExecuteScript(string.Format("window.scrollBy(0, {0})", c));
                if (TryFindElement(by, out element))
                {
                    bool visible = IsElementVisible(element);
                    if (visible)
                    {
                        js.ExecuteScript(string.Format("window.scrollTo(0, {0})", element.Location.Y - 250));
                        Thread.Sleep(2000);
                        //long y = (long)ExecuteJS("return window.scrollY");
                        //if (element.Location.Y - 200 < y) return true;                        
                        return true;
                    }
                }

                if (maxLoop == 0) return false;

                Thread.Sleep(200);
            }
        }

        public bool TryFindElement(By by, out IWebElement element)
        {
            try
            {
                element = driver.FindElement(by);
            }
            catch (NoSuchElementException ex)
            {
                element = null;
                return false;
            }

            return true;
        }

        public bool IsElementVisible(IWebElement element)
        {
            return element.Displayed && element.Enabled;
        }

        private void Scroll(int c)
        {
            // 스크롤 다운icon-sprite icon-gender-f
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            for (int i = 0; i < c; i++)
            {                
                js.ExecuteScript("window.scrollBy(0, 200)");
                Thread.Sleep(500);
            }
        }

        private void ScrollTo(int c)
        {
            // 스크롤 다운icon-sprite icon-gender-f
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;         
            js.ExecuteScript(string.Format( "window.scrollBy(0, {0})", c));
            Thread.Sleep(500);            
        }

        private void ScrollAt(int c)
        {
            // 스크롤 다운icon-sprite icon-gender-f
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(string.Format("window.scrollTo(0, {0})", c));
            Thread.Sleep(500);
        }

        private void Scroll(string script, int c)
        {
            // 스크롤 다운icon-sprite icon-gender-f
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            for (int i = 0; i < c; i++)
            {
                js.ExecuteScript(script);
                Thread.Sleep(500);
            }
        }

        private void ScrollByInfinite(int c, int delay)
        {
            // 스크롤 다운icon-sprite icon-gender-f
            long lastScrollHeight = 0;
            while (true) {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript(string.Format("window.scrollBy(0, document.body.scrollHeight)", c));
                Thread.Sleep(delay);

                long currentScrollHeight = (long)js.ExecuteScript("return document.body.scrollHeight");

                if (lastScrollHeight != 0 && currentScrollHeight == lastScrollHeight) {
                    break;
                }
                lastScrollHeight = currentScrollHeight;
            }
        }

        private void executeJS(string script) {
            IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
            js.ExecuteScript(script);
        }

        //  브라우저 종료
        private void CloseBrowser()
        {

            if (driver != null)
            {
                try
                {
                    driver.Close();
                    driver.Quit();
                }
                catch (Exception ex)
                {
                    if (ex is InvalidOperationException || ex is NoSuchWindowException)
                    {
                        ProcessKillByName("chromedriver");
                    }
                }
            }
            else
            {
                ProcessKillByName("chromedriver");
            }
        }

        public static void ProcessKillByName(string name)
        {
            Process[] processList = Process.GetProcessesByName(name);
            if (processList.Length > 0)
            {
                for (int i = 0; i < processList.Length; i++)
                {
                    processList[i].Kill();
                }
            }
        }

        private bool IsElementPresent(By by)
        {
            try
            {
                driver.FindElement(by);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        // 쿠키 삭제
        private void DeleteCookie()
        {            
            driver.Manage().Cookies.DeleteAllCookies();
        }

        // 문자 프로퍼티 반환
        private string getProperty(string key) {
            return globalConfig[key];
        }

        // 숫자 프로퍼티 반환
        private int getIntProperty(string key)
        {
            return int.Parse(globalConfig[key]);
        }

        // 최소-최대 레인지 반환
        private int[] getRangeProperty(string key)
        {
            int[] r = new int[2];
            r[0] = int.Parse(globalConfig[key].Split('-')[0]);
            r[1] = int.Parse(globalConfig[key].Split('-')[1]);
            return r;
        }

        // 최소-최대 프로퍼티에서 랜덤한값 추출 반환  
        private int getRandomRangeProperty(string key)
        {
            int[] r = new int[2];
            r[0] = int.Parse(globalConfig[key].Split('-')[0]);
            r[1] = int.Parse(globalConfig[key].Split('-')[1]);
            return GetRandomValue(r[0], r[1]);            
        }

        private void Run() {

            // 라이센스 체크
            isValidLicense();
            
            // 키워드 모드
            if ("K".Equals(mode))
            {
                RunKeyWordMode();
            }
            // 닉네임 모드
            else if ("N".Equals(mode))
            {
                RunNickNameMode();
            }
            else {
                Console.WriteLine("[ERROR] 존재하지 않는 모드가 입력되었습니다.");
            }

        }

        // 확률에 의한 랜덤뽑기
        public bool RandomPick(int count) 
        {
            List<bool> l = new List<bool>();
            for (int i = 0; i < 100; i++) 
            {
                if (i <= count)
                {
                    l.Add(true);
                }
                else {
                    l.Add(false);
                }
            }

            Shuffle(l);

            var index = new Random().Next(l.Count);
            bool randomItem = l[index];

            return randomItem;
        }


        // 닉네임 작업 모드
        private void RunNickNameMode()
        {

            string currentIp = null;

            int ipChangeAfterDelayMin = int.Parse(globalConfig["ip.change.after.delay"].Split('-')[0]);
            int ipChangeAfterDelayMax = int.Parse(globalConfig["ip.change.after.delay"].Split('-')[1]);
            int scrollReapeatCntMin = int.Parse(globalConfig["scroll.reapeat.cnt"].Split('-')[0]);
            int scrollReapeatCntMax = int.Parse(globalConfig["scroll.reapeat.cnt"].Split('-')[1]);
            int subMode = getIntProperty("system.mode.sub");



            LB_START:

            try
            {

                while (true)
                {
                    // 닉네임 리스트를 가져온다.
                    List<NickKeyowrd> nickKeyowrdList = sqlUtil.SelectRemainNickKeywordList();

                    // 작업대상이 없을 경우
                    if (nickKeyowrdList.Count == 0)
                    {
                        Console.WriteLine("[INFO] 작업을 수행할 대상이 존재하지 않습니다.");
                        Thread.Sleep(1000 * getIntProperty("user.work.empty.delay"));
                        continue;
                    }

                    String userAgent = userAgentList[new Random().Next(userAgentList.Count)];
                    Console.WriteLine(string.Format("[INFO] 접속 에이전트 {0}", userAgent));
                    driver = MakeDriver(false, userAgent);

                    currentIp = GetExternalIPAddress();
                    Console.WriteLine(string.Format("[INFO] 현재아이피 {0}", GetExternalIPAddress()));
                    // 아이피 삽입
                    sqlUtil.InsertIpHistory(currentIp);


                    // 닉네임 단건만 디스팅트 처리
                    nickKeyowrdList = nickKeyowrdList.DistinctBy(x => x.nickNm).ToList();

                    // 리스트 셔플
                    Shuffle(nickKeyowrdList);



                    for (int i = 0; i < nickKeyowrdList.Count; i++)
                    {

                        NickKeyowrd nickKeyowrd = nickKeyowrdList[i];

                        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>> 작업 닉네임 [{0}] <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", nickKeyowrd.nickNm);

                        Console.WriteLine("[INFO] 네이버 메인으로 이동");
                        driver.Navigate().GoToUrl("https://m.naver.com");

                        Thread.Sleep(500);

                        WaitForVisible(driver, By.Id("MM_SEARCH_FAKE"), 10);


                        Console.WriteLine("[INFO] 검색어 클릭");
                        Actions actions = new Actions(driver);
                        IWebElement e = null;
                        actions.MoveToElement(driver.FindElement(By.Id("MM_SEARCH_FAKE"))).Click().Perform();
                        Thread.Sleep(500);
                        e = driver.FindElement(By.Id("query"));
                        Thread.Sleep(500);

                        WaitForVisible(driver, By.Id("query"), 10);

                        Console.WriteLine("[INFO] 닉네임 입력");

                        // 확률에 의한 샾 붙이기
                        if (RandomPick(getRandomRangeProperty("nick.keyword.shap.percentage"))) {
                            nickKeyowrd.keyword = "#" + nickKeyowrd.keyword;
                        }


                        e.SendKeys(nickKeyowrd.keyword);
                        Thread.Sleep(500);
                        e.SendKeys(OpenQA.Selenium.Keys.Enter);
                        Thread.Sleep(500);

                        WaitForVisible(driver, By.Id("nx_query"), 10);

                        IWebElement element = null;

                        // 인플루언서 찾기
                        string moreCss = ".keyword_more";
                        // 최대 클릭 카운트
                        int moreClickCnt = 2;
                        while (true)
                        {
                            try
                            {
                                Console.WriteLine("[INFO] 인플루언서 찾기");
                                element = driver.FindElement(By.CssSelector(string.Format("[data-alarm-name='{0}']", nickKeyowrd.nickNm.Replace("@", ""))));

                                // 기본모드
                                if (subMode == 0)
                                {
                                    element = element.FindElement(By.CssSelector(".detail_box"));
                                    ScrollAt(element.Location.Y - 200);
                                    element.Click();
                                    WaitForVisible(driver, By.CssSelector(".ContentEnd__profile___3wPJw"), 10);
                                }
                                // 태그 클릭하는 모드
                                else if (subMode == 1)
                                {
                                    element = element.FindElement(By.CssSelector(".detail_box .tag_inner a"));
                                    ScrollAt(element.Location.Y - 200);
                                    element.Click();
                                    WaitForVisible(driver, By.CssSelector("#common_header"), 10);

                                    Thread.Sleep(1500);

                                    while(true)
                                    {
                                        if (ScrollByTry(new Random().Next(1000, 1000), By.CssSelector("[class^='Footer__top']"), 20))
                                        {
                                            try
                                            {
                                                // 버튼이 존재할 경우 클릭
                                                element = driver.FindElement(By.CssSelector("[class^='MoreButton__root']"));
                                                ScrollAt(element.Location.Y - 200);
                                                element.Click();
                                            }
                                            catch(Exception ex)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                    List<IWebElement> elements = driver.FindElements(By.CssSelector("[class^='ChallengeHistory__area_article'] ")).ToList();
                                    elements.Reverse();
                                    element = elements[Int32.Parse(nickKeyowrd.option1) - 1];
                                    element = element.FindElement(By.CssSelector("[class^='ChallengeContents__root']"));
                                    ScrollAt(element.Location.Y - 200);
                                    element.Click();
                                }

                                Console.WriteLine("[INFO] 포스트 클릭 후 대기");
                                Thread.Sleep(getRandomRangeProperty("post.click.after.delay") * 1000);

                                Console.WriteLine("[INFO] 포스트 스크롤링 처리");

                                try
                                {

                                    driver.FindElement(By.CssSelector(".instagram-media"));
                                    Scroll("document.querySelector('.ContentEnd__content___3M7l2').scrollBy(0, 200)", GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));
                                    
                                }
                                catch
                                {
                                    // 아이프레임 전환
                                    driver.SwitchTo().Frame(0);
                                    Scroll(GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));
                                }

                                goto LB_END;
                            }
                            catch (Exception ex)
                            {
                                element = driver.FindElement(By.CssSelector(moreCss));
                                //ScrollAt(element.Location.Y - 100);

                                //Thread.Sleep(1000);

                                element.Click();
                                moreClickCnt--;
                                Thread.Sleep(2000);

                                // 클릭 2번을 소진하면 브레이크
                                if (moreClickCnt == 0) break;

                                // 키워드가 #으로 시작하면 break;
                                // 키워드#으로 할경우 더보기하면 바로 인플루언서 홈으로가고 아닌경우 한번 더보기 되어짐
                                if (nickKeyowrd.keyword.StartsWith("#"))
                                {
                                    break;
                                }
                                else
                                {
                                    Console.WriteLine("[INFO] 인플루언서 더 찾아보기 클릭");
                                    moreCss = ".keyword_more._more";
                                    continue;
                                }
                            }
                        }

                        // 인플루 언서 홈 대기
                        Console.WriteLine("[INFO] 인플루언서 홈 대기");
                        WaitForVisible(driver, By.CssSelector(".mod_header_lnb"), 10);

                        // 스크롤 제일 밑에 까지 내리기
                        Console.WriteLine("[INFO] 무한 스크롤링");
                        ScrollByInfinite(new Random().Next(4000, 5000), new Random().Next(100, 200));

                        Thread.Sleep(2000);

                        // 인플루언서 찾기
                        try
                        {
                            Console.WriteLine("[INFO] 인플루언서 찾기");
                            element = driver.FindElement(By.CssSelector(string.Format("[data-alarm-name='{0}']", nickKeyowrd.nickNm.Replace("@", ""))));
                            element = element.FindElement(By.CssSelector(".detail_box"));

                            ScrollAt(element.Location.Y - 200);


                                //Actions action = new Actions(driver);
                                //action = action.MoveToElement(element);

                            //Thread.Sleep(5000);

                            //action.Click().Perform();

                            element.Click();

                            WaitForVisible(driver, By.CssSelector(".ContentEnd__profile___3wPJw"), 10);

                            Console.WriteLine("[INFO] 포스트 클릭 후 대기");
                            Thread.Sleep(getRandomRangeProperty("post.click.after.delay") * 1000);


                            Console.WriteLine("[INFO] 포스트 스크롤링 처리");

                            try
                            {

                                driver.FindElement(By.CssSelector(".instagram-media"));
                                Scroll("document.querySelector('.ContentEnd__content___3M7l2').scrollBy(0, 200)", GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));

                            }
                            catch
                            {
                                // 아이프레임 전환
                                driver.SwitchTo().Frame(0);
                                Scroll(GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));
                            }

                        }
                        catch
                        {
                            Console.WriteLine("[INFO] 인플루언서를 찾을 수 없음");
                        }

                        LB_END :

                        sqlUtil.UpdateNickKeyWordWorkCnt(nickKeyowrd.workYmd, nickKeyowrd.nickNm, nickKeyowrd.keyword);

                        if (i == nickKeyowrdList.Count - 1)
                        {
                            Console.WriteLine("[INFO] 쿠키 삭제");
                            DeleteCookie();

                            Console.WriteLine("[INFO] 브라우저 닫기");
                            CloseBrowser();

                            ChangeIp(ipChangeAfterDelayMin, ipChangeAfterDelayMax, currentIp);
                        }
                    }
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Console.WriteLine("[INFO] 글로벌 익섹셥 발생");
                ChangeIp(ipChangeAfterDelayMin, ipChangeAfterDelayMax, currentIp);
                Console.WriteLine("[INFO] 브라우저 닫기");
                CloseBrowser();
                goto LB_START;
            }
        }

        // 키워드 작업 모드
        private void RunKeyWordMode()
        {

            //int loopCnt = int.Parse(globalConfig["loop.count"]);
            int hashMinWorkCnt = int.Parse(globalConfig["hash.work.cnt"].Split('-')[0]);
            int hashMaxWorkCnt = int.Parse(globalConfig["hash.work.cnt"].Split('-')[1]);
            int ipChangeAfterDelayMin = int.Parse(globalConfig["ip.change.after.delay"].Split('-')[0]);
            int ipChangeAfterDelayMax = int.Parse(globalConfig["ip.change.after.delay"].Split('-')[1]);
            int scrollReapeatCntMin = int.Parse(globalConfig["scroll.reapeat.cnt"].Split('-')[0]);
            int scrollReapeatCntMax = int.Parse(globalConfig["scroll.reapeat.cnt"].Split('-')[1]);

            string currentIp = null;


            LB_START:

            try
            {
                while (true)
                {
                    //LB_START:

                    // 오늘 해야할 작업이 남은 유저를 뽑아온다.
                    List<User> userList = sqlUtil.SelectWorkRemainUserList();

                    // 작업대상 유저가 없을 경우 브레이크
                    if (userList.Count == 0)
                    {
                        Console.WriteLine("[INFO] 작업을 수행할 유저가 존재하지 않습니다.");
                        Thread.Sleep(1000 * getIntProperty("user.work.empty.delay"));
                        continue;
                    }


                    String userAgent = userAgentList[new Random().Next(userAgentList.Count)];
                    Console.WriteLine(string.Format("[INFO] 접속 에이전트 {0}", userAgent));
                    driver = MakeDriver(false, userAgent);

                    currentIp = GetExternalIPAddress();
                    Console.WriteLine(string.Format("[INFO] 현재아이피 {0}", GetExternalIPAddress()));
                    // 아이피 삽입
                    sqlUtil.InsertIpHistory(currentIp);

                    for (int i = 0; i < userList.Count; i++)
                    {
                        User user = userList[i];

                        Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>> 작업유저 [{0}] <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<", user.nickNm);

                        Console.WriteLine("[INFO] 네이버 메인으로 이동");
                        driver.Navigate().GoToUrl("https://m.naver.com");

                        Thread.Sleep(500);

                        WaitForVisible(driver, By.Id("MM_SEARCH_FAKE"), 10);


                        Console.WriteLine("[INFO] 검색어 클릭");
                        Actions actions = new Actions(driver);
                        IWebElement e = null;
                        actions.MoveToElement(driver.FindElement(By.Id("MM_SEARCH_FAKE"))).Click().Perform();
                        Thread.Sleep(500);
                        e = driver.FindElement(By.Id("query"));
                        Thread.Sleep(500);

                        WaitForVisible(driver, By.Id("query"), 10);

                        Console.WriteLine("[INFO] 인플루언서 아이디 입력");
                        e.SendKeys(user.nickNm);
                        Thread.Sleep(500);
                        e.SendKeys(OpenQA.Selenium.Keys.Enter);
                        Thread.Sleep(500);

                        WaitForVisible(driver, By.CssSelector(".influencer_wrap .creator_wrap .user_area"), 10);

                        Console.WriteLine("[INFO] 프로필 클릭");
                        e = driver.FindElement(By.CssSelector(".influencer_wrap .creator_wrap .user_area"));
                        e.Click();
                        Thread.Sleep(500);

                        Console.WriteLine("[INFO] 인플루언서 홈 화면 로드 될때 까지 대기");
                        try
                        {
                            WaitForVisible(driver, By.CssSelector(".RecentUploadContentImageArticle__root___2MoKg"), 10);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[ERROR] 지원하지 않는 브라우저 입니다.");
                            Console.WriteLine("[INFO] 브라우저 닫기");
                            CloseBrowser();
                            goto LB_START;
                        }


                        Console.WriteLine("[INFO] 인플루언서 홈 로드 완료");


                        Console.WriteLine("[INFO] 키워드 챌린지 탭 클릭");
                        e = driver.FindElement(By.XPath("//*[@id='app']/div[1]/div/div/div[3]/div/div/a[2]"));

                        e.Click();

                        WaitForVisible(driver, By.CssSelector("#keyword_list"), 10);

                        Console.WriteLine("[INFO] 키워드 리스트 조회");

                        IReadOnlyCollection<IWebElement> elements = null;
                        IReadOnlyCollection<IWebElement> keywordElements = driver.FindElements(By.CssSelector(".Keyword__btn___1f2BW"));

                        // 유저 해시리스트 조회
                        user.hashList = sqlUtil.SelectWorkRemainHashList(user.nickNm);

                        // 해시키워드 셔플 할지 여부
                        if ("Y".Equals(getProperty("hash.click.random")))
                        {
                            user.hashList = user.hashList.OrderBy(elem => Guid.NewGuid()).ToList();
                        }

                        if (user.hashList == null || user.hashList.Count == 0)
                        {
                            Console.WriteLine("[INFO] 설정된 해시태그가 없습니다.");
                            return;
                        }

                        Console.WriteLine("[INFO] 총 키워드 개수 : {0}", keywordElements.Count);

                        List<string> hashNms = user.hashList
                            .Select(item => item.hashNm)
                            .ToList();

                        Console.WriteLine("[INFO] 설정된 키워드 : {0}", string.Join(",", hashNms));

                        List<String> list = null;

                        if (keywordElements.Count == 0)
                        {
                            Console.WriteLine("[ERROR] 챌린지탭에 해시태그가 없습니다.");
                            return;
                        }

                        foreach (Hash hash in user.hashList)
                        {

                            IWebElement hashElement = driver.FindElements(By.CssSelector(".Keyword__btn___1f2BW")).Where(el => el.Text.Contains("#" + hash.hashNm)).FirstOrDefault();

                            if (hashElement == null)
                            {
                                Console.WriteLine("[ERROR] 선택된 해시키워드[{0}]가 없습니다", hash.hashNm);
                                continue;
                            }
                            //IWebElement hashElement = driver.FindElement(By.XPath(".Keyword__btn___1f2BW:contains('#" + hash.hashNm + "')"));

                            /* IWebElement hashElement = keywordElements
                                      .Select(x => x)
                                      .Where(x => x.Text.Equals("#" + hash.hashNm))
                                      .First();*/
                            //if (string.Join(",", user.hashList).IndexOf(hashElement.Text.Replace("#", "")) == -1) continue;
                            //*[@id="143829555712000"]
                            Console.WriteLine("[INFO] 해시태그클릭 [{0}]", hashElement.Text.Replace("#", ""));

                            //Console.WriteLine("▶ 해시태그 클릭");
                            //IWebElement hashTag = list[new Random().Next(list.Count)];
                            actions = new Actions(driver);
                            actions = actions.MoveToElement(hashElement);

                            // 이동이 늦는 문제로 이동 후 2초 대기시간 부여
                            Thread.Sleep(5000);

                            actions.Click().Perform();

                            Thread.Sleep(2000);

                            // 섞기
                            //ShuffleMe(list);

                            int workCnt = GetRandomValue(hashMinWorkCnt, hashMaxWorkCnt);
                            Console.WriteLine(string.Format("[INFO] 해시태그 작업량 > {0}", workCnt));

                            WaitForVisible(driver, By.CssSelector(".ChallengeHistory__area_article___sWmKY"), 30);
                            Thread.Sleep(1000);

                            // 포스팅 엘리먼트
                            elements = driver.FindElements(By.CssSelector(".KeywordChallenge__root___qMD-g"));


                            if (!CheckImageLoad(elements))
                            {
                                Console.WriteLine("[ERROR] 이미지가 로드되지 않고 더 보기가 동작하지 않는 것으로 판단하여 닫고 다시시작");
                                CloseBrowser();
                                goto LB_START;
                            }

                            int initElementCnt = elements.Count;
                            // 더보기
                            while (IsElementPresent(By.CssSelector(".MoreButton__root___knmp1")) && elements.Count < workCnt)
                            {
                                driver.FindElement(By.CssSelector(".MoreButton__root___knmp1")).Click();
                                Thread.Sleep(getRandomRangeProperty("more.click.delay") * 1000);
                                Thread.Sleep(1000);
                                elements = driver.FindElements(By.CssSelector(".KeywordChallenge__root___qMD-g"));
                                //
                                if (!CheckImageLoad(elements))
                                {
                                    Console.WriteLine("[ERROR] 이미지가 로드되지 않고 더 보기가 동작하지 않는 것으로 판단하여 닫고 다시시작");
                                    CloseBrowser();
                                    goto LB_START;
                                }
                            }

                            list = elements.Select(item => item.GetAttribute("id")).ToList();

                            if (elements.Count > workCnt)
                            {
                                list = elements.Take(workCnt).Select(item => item.GetAttribute("id")).ToList();
                            }


                            try
                            {

                                foreach (String id in list)
                                {
                                    IWebElement element = driver.FindElement(By.Id(id));
                                    //IWebElement rootElemnet = element.FindElement(By.XPath("//select[@name='day']//ancestor::div[contains(@id, '" + id + "')]"));

                                    // NBLOG : 블로그
                                    // INSTAGRAM : 인스타그램
                                    // NPOST : 네이버 포스트
                                    // NTV   : 네이버 TV
                                    // YOUTUBE : 유튜브  
                                    // TWITTER : 트위터
                                    string challengeType = null;

                                    try
                                    {
                                        challengeType = element.FindElement(By.CssSelector("i[class*='__icon___']")).GetAttribute("aria-label");
                                    }
                                    catch (Exception xe)
                                    {
                                        // 트위터일 경우 이쪽으로 오게됨.
                                        challengeType = "TWITTER";
                                    }



                                    Thread.Sleep(2000);

                                    Console.WriteLine(string.Format("[INFO] [{0}] 챌린지 클릭 ( {1} )", challengeType, element.FindElement(By.CssSelector(".KeywordChallenge__category___2eNAE")).Text));


                                    IWebElement postElemnet = null;
                                    // 포스트 클릭
                                    if (challengeType.Equals("NBLOG"))
                                    {
                                        postElemnet = element.FindElement(By.CssSelector(".ChallengeBlogPost__info_area___1F_-p"));
                                    }
                                    else if (challengeType.Equals("INSTAGRAM") || challengeType.Equals("TWITTER"))
                                    {
                                        postElemnet = element.FindElement(By.CssSelector(".ChallengeContents__root___1zlfP"));
                                    }
                                    else if (challengeType.Equals("NPOST"))
                                    {
                                        postElemnet = element.FindElement(By.CssSelector(".ChallengeContents__root___1zlfP"));
                                    }
                                    else if (challengeType.Equals("NTV") || challengeType.Equals("YOUTUBE"))
                                    {
                                        postElemnet = element.FindElement(By.CssSelector(".ChallengeYoutubeNtv__image___tJBxd"));
                                    }



                                    // 포스트로 이동 후 클릭
                                    actions = new Actions(driver);
                                    actions.MoveToElement(postElemnet).Click().Perform();


                                    Thread.Sleep(2000);

                                    Console.WriteLine("[INFO] 레이어팝업 오픈 대기");
                                    IWebElement frame = null;

                                    Console.WriteLine("[INFO] 포스트 클릭 후 대기");
                                    Thread.Sleep(getRandomRangeProperty("post.click.after.delay") * 1000);

                                    // Thread.Sleep(1000);
                                    // 레이어 대기 및 스크롤
                                    if (challengeType.Equals("NBLOG") || challengeType.Equals("NPOST") || challengeType.Equals("NTV"))
                                    {
                                        WaitForVisible(driver, By.CssSelector(".ContentEnd__naver_service_iframe___2CHqZ"), 10);
                                        Thread.Sleep(1000);
                                        frame = driver.FindElement(By.CssSelector(".ContentEnd__naver_service_iframe___2CHqZ"));
                                        executeJS("document.querySelector('.ContentEnd__naver_service_iframe___2CHqZ').setAttribute('name', 'Layer_Frame')");

                                        // 아이프레임 전환
                                        driver.SwitchTo().Frame(frame.GetAttribute("name"));

                                        Thread.Sleep(1000);

                                        Console.WriteLine("[INFO] 레이어팝업 스크롤링 처리");
                                        Scroll(GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));

                                    }
                                    else if (challengeType.Equals("INSTAGRAM"))
                                    {
                                        WaitForVisible(driver, By.CssSelector(".instagram-media"), 10);
                                        Thread.Sleep(1000);

                                        Scroll("document.querySelector('.ContentEnd__content___3M7l2').scrollBy(0, 200)", 10);
                                    }
                                    else if (challengeType.Equals("TWITTER"))
                                    {
                                        WaitForVisible(driver, By.CssSelector(".twitter-tweet"), 10);
                                        Thread.Sleep(1000);

                                        Scroll("document.querySelector('.ContentEnd__content___3M7l2').scrollBy(0, 200)", 10);
                                    }
                                    else if (challengeType.Equals("YOUTUBE"))
                                    {
                                        WaitForVisible(driver, By.CssSelector(".YoutubeEmbed__youtube_iframe___S3Zi0"), GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));
                                        Thread.Sleep(1000);

                                        Scroll("document.querySelector('.ContentEnd__content___3M7l2').scrollBy(0, 200)", GetRandomValue(scrollReapeatCntMin, scrollReapeatCntMax));
                                    }



                                    driver.SwitchTo().DefaultContent();

                                    Console.WriteLine("[INFO] 레이어 닫기");

                                    // 닫기버튼이 사라져서 뒤로가기로 변경
                                    //e = driver.FindElement(By.CssSelector(".ContentEnd__close___2ILuX"));
                                    //e.Click();
                                    driver.Navigate().Back();

                                    Thread.Sleep(250);

                                    sqlUtil.UpdateHashWorkCnt(hash.workYmd, user.nickNm, hash.hashNm);

                                    Thread.Sleep(250);
                                }

                            }
                            catch (Exception ex)
                            {
                                if (ex.GetType().IsInstanceOfType(new OpenQA.Selenium.WebDriverTimeoutException()))
                                {
                                    Console.WriteLine("[ERROR] 이미지 로딩이 불충분 한 것으로 판단하여 리스트 전체를 넘김 처리");
                                }
                            }

                        }

                        if (i == userList.Count - 1)
                        {
                            Console.WriteLine("[INFO] 쿠키 삭제");
                            DeleteCookie();

                            Console.WriteLine("[INFO] 브라우저 닫기");
                            CloseBrowser();

                            ChangeIp(ipChangeAfterDelayMin, ipChangeAfterDelayMax, currentIp);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);

                Console.WriteLine("[INFO] 글로벌 익섹셥 발생");
                ChangeIp(ipChangeAfterDelayMin, ipChangeAfterDelayMax, currentIp);
                goto LB_START;
            }
        }

        private void ChangeIp(int ipChangeAfterDelayMin, int ipChangeAfterDelayMax, string ip)
        {
            if (globalConfig["ip.change.use"].Equals("Y"))
            {
                Console.WriteLine("[INFO] 아이피 변경 시작");

                while (true)
                {
                    ChangeIP();
                    ip = GetExternalIPAddress();
                    int reuseSecond = getIntProperty("ip.reuse.minutes");
                    if (reuseSecond > 0)
                    {
                        List<IpHistory> ipHistoryList = sqlUtil.selectIpHistory(reuseSecond, ip);
                        if (ipHistoryList.Count > 0)
                        {
                            Console.WriteLine(string.Format("[INFO] {0}분전에 사용된 아이피[{1}] 재변경을 합니다.", reuseSecond, ip));
                            continue;
                        }

                    }
                    break;
                }

                Thread.Sleep(GetRandomValue(ipChangeAfterDelayMin, ipChangeAfterDelayMax) * 1000);
                Console.WriteLine(string.Format("[INFO] 변경 아이피 {0}", GetExternalIPAddress()));
            }            
        }

        private bool CheckImageLoad(IReadOnlyCollection<IWebElement> elements)
        {
            foreach (IWebElement element in elements)
            {
                ScrollTo(element.Location.Y);
                string url = element.FindElement(By.TagName("img")).GetAttribute("src");
                if (url == null || url.Trim().Length == 0) return false;
                else return true;
            }

            return true;
        }

        public void ShuffleMe<T>(IList<T> list)
        {
            Random random = new Random();
            int n = list.Count;
            for (int i = list.Count - 1; i > 1; i--)
            {
                int rnd = random.Next(i + 1);
                T value = list[rnd];
                list[rnd] = list[i];
                list[i] = value;
            }
        }

        private void WaitForVisible(IWebDriver driver, By by, int seconds)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds((double)seconds));
                wait.Until<IWebElement>(ExpectedConditions.ElementExists(by));
            }
            catch (Exception e) {
                throw e;
            }
            
        }


        // 시작버튼 클릭
        private void Button1_Click(object sender, EventArgs e)
        {
            browserWorker = new Thread(Run);
            browserWorker.Start();            
            //ChangeIP();
        }

        public static string GetExternalIPAddress()
        {
            string url = "https://ip.pe.kr/";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            string resResult = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                StreamReader readerPost = new StreamReader(response.GetResponseStream(), System.Text.Encoding.UTF8, true);
                resResult = readerPost.ReadToEnd();
            }

            Regex regexp = new Regex(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}");
            string IP = regexp.Matches(resResult)[0].ToString();


            //int ingNO = resResult.IndexOf("조회 IP");
            //string varTemp = resResult.Substring(ingNO, 50);
            //string realIP = Parsing(Parsing(varTemp, "Current IP Address: ", 1), "</body>", 0).Trim();
            return IP;

        }


        // 아이피 변경
        private void ChangeIP()
        {
            Console.WriteLine("[INFO] 데이터 OFF");
            DisAbleData();
            Thread.Sleep(3000);

            Console.WriteLine("[INFO] 데이터 ON");
            EnAbleData();
            Thread.Sleep(3000);            
        }

        public static int GetRandomValue(int min, int max)
        {
            return new Random().Next(min, max);
        }


        private static string cmd = Application.StartupPath + @"\adb\adb.exe";

        public static void EnAbleData()
        {
            // String cmd = "C:\\Users\\JHJ\\Downloads\\adb\\adb.exe";            
            Process process = new Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = cmd;//设定程序名  
            process.StartInfo.Arguments = " shell svc data enable";
            process.StartInfo.UseShellExecute = false; //关闭shell的使用  
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入  
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
            process.StartInfo.RedirectStandardError = true; //重定向错误输出  
            process.StartInfo.CreateNoWindow = true;//设置不显示窗口  
            process.Start();
            process.WaitForExit();
           
        }

        public static void DisAbleData()
        {
            Process process = new Process();
            process.StartInfo = new System.Diagnostics.ProcessStartInfo();
            process.StartInfo.FileName = cmd;//设定程序名  
            process.StartInfo.Arguments = " shell svc data disable";
            process.StartInfo.UseShellExecute = false; //关闭shell的使用  
            process.StartInfo.RedirectStandardInput = true; //重定向标准输入  
            process.StartInfo.RedirectStandardOutput = true; //重定向标准输出  
            process.StartInfo.RedirectStandardError = true; //重定向错误输出  
            process.StartInfo.CreateNoWindow = true;//设置不显示窗口  
            process.Start();
            process.WaitForExit();
          
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        // 갱신 버튼
        private void button2_Click(object sender, EventArgs e)
        {
            if ("K".Equals(mode))
            {
                List<Hash> hashList = sqlUtil.SelectHashList(textBox1.Text, textBox2.Text);
                dt.Rows.Clear();
                long sum = 0;
                foreach (Hash hash in hashList)
                {
                    sum += hash.workCnt;
                    dt.Rows.Add(hash.workYmd, hash.nickNm, hash.hashNm, hash.totCnt, hash.workCnt);
                }

                label4.Text = string.Format("{0}", sum);
            }
            else if ("N".Equals(mode))
            {
                List<NickKeyowrd> nickKeyowrdList = sqlUtil.SelectNickKeywordList(textBox1.Text, textBox2.Text);
                dt.Rows.Clear();
                long sum = 0;
                foreach (NickKeyowrd nickKeyowrd in nickKeyowrdList)
                {
                    sum += nickKeyowrd.workCnt;
                    dt.Rows.Add(nickKeyowrd.workYmd, nickKeyowrd.nickNm, nickKeyowrd.keyword, nickKeyowrd.totCnt, nickKeyowrd.workCnt);
                }

                label4.Text = string.Format("{0}", sum);
            }
            else
            {
                Console.WriteLine("[ERROR] 존재하지 않는 모드가 입력되었습니다.");
            }


        }

        public List<string> GetMacAddr()
        {

            List<string> list = new List<string>();

            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                list.Add(networkInterface.GetPhysicalAddress().ToString());
            }

            return list;
        }


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            
        }

        //AE_S128 복호화
        public String AESDecrypt128(String Input, String key)
        {
            RijndaelManaged RijndaelCipher = new RijndaelManaged();

            byte[] EncryptedData = Convert.FromBase64String(Input);
            byte[] Salt = Encoding.ASCII.GetBytes(key.Length.ToString());

            PasswordDeriveBytes SecretKey = new PasswordDeriveBytes(key, Salt);
            ICryptoTransform Decryptor = RijndaelCipher.CreateDecryptor(SecretKey.GetBytes(32), SecretKey.GetBytes(16));
            MemoryStream memoryStream = new MemoryStream(EncryptedData);
            CryptoStream cryptoStream = new CryptoStream(memoryStream, Decryptor, CryptoStreamMode.Read);

            byte[] PlainText = new byte[EncryptedData.Length];

            int DecryptedCount = cryptoStream.Read(PlainText, 0, PlainText.Length);

            memoryStream.Close();
            cryptoStream.Close();

            string DecryptedData = Encoding.Unicode.GetString(PlainText, 0, DecryptedCount);

            return DecryptedData;
        }

        public void isValidLicense()
        {
            if ("SHOWMETHEMONEY".Equals(license)) return;

            if (license.Length == 0)
            {
                throw new Exception("License Not Vaild!");
            }
            string[] licenseArr = license.Split('^');
            if (licenseArr.Length != 3)
            {
                throw new Exception("License Not Vaild!");
            }
            if (!licenseArr[0].Equals("INF"))
            {
                throw new Exception("License Not Vaild!");
            }
            if (!GetMacAddr().Contains(licenseArr[1]))
            {
                throw new Exception("License Not Vaild!");
            }
            if (licenseArr[2].CompareTo(DateTime.Now.ToString("yyyyMMddHHmmss")) < 0)
            {
                throw new Exception("License Not Vaild!");
            }
        }

        public void Shuffle<T>(List<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


    }


    
}
