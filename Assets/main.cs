using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Facebook.MiniJSON;

using System.Text;
using System.IO;
using System.Security.Cryptography;
using System;

using System.Net;
using System.Security.Cryptography.X509Certificates;

using System.Xml;

public enum eSendType
{
    FIRST_INFO,
    LOGIN_INFO,
    BUY_INFO,
}

public class main : MonoBehaviour {

    string PurchaseInfoServerURL = "https://admin.vm-casino.com/paymentFail.do";

    // Use this for initialization
    void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("a");
            SendInfoToGameServer(eSendType.BUY_INFO, PurchaseInfoServerURL, "{\"error_code\":1383010,\"error_message\":\"User canceled the order.\",\"callback_id\":\"1\"}");
        }
	}

    public static void SendInfoToGameServer(eSendType sendtype, string strURL = "", string sendinfo = "")
    {

        string strSendURL = strURL;

        HttpWebRequest objHttpWebRequest = (HttpWebRequest)WebRequest.Create(strSendURL);
        HttpWebResponse objHttpWebResponse = null;
        Stream objRequestStream = null;
        Stream objResponseStream = null;

        //if (sendtype == eSendType.FIRST_INFO || sendtype == eSendType.BUY_INFO)
        //{
        //    strSendURL = strURL;
        //}
        //else if (sendtype == eSendType.LOGIN_INFO)
        //{
        //    strSendURL = NetworkMng._Inst.DeviceInfoServerURL;
        //}

        Debug.Log("SendInfoToGameServer..." + sendtype + ", " + strSendURL + ", " + sendinfo);// + _type);

        string strUniqueDeviceKey = "uniqueidtest123";// Util.GetUniqueID();
        string strFacebookID = "123";

        //if (Main._Inst.currLoginType == eLOGIN_TYPE.FACEBOOK)
        //{
        //    //Debug.Log("FaceBookUtil._Inst.IsLoggedIn : " + FaceBookUtil._Inst.IsLoggedIn);
        //    strFacebookID = FaceBookUtil._Inst.UserId;
        //}

        
        
        

        // SSL(TLS) 인증서 세팅하는 부분(인증서를 txt로 바꿔야 함 .crt -> .txt)
        TextAsset certText = Resources.Load<TextAsset>("Certificate/_wildcard_vm-casino_com");
        X509Certificate2 certFile = new X509Certificate2(certText.bytes);
        objHttpWebRequest.ClientCertificates.Add(certFile);

        // 콜백함수 등록하는 부분
        System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => { return true; };

        string strPlatform = "google";
#if UNITY_ANDROID
        strPlatform = "google";
#elif UNITY_IPHONE
        strPlatform = "apple";
#endif
        string strSendData = "";
        if (sendtype == eSendType.FIRST_INFO)
        {
            // 데이터 생성(최초접속시 보낼거)
            string strProvider = strPlatform;
            strSendData = string.Format("platform={0}&uuid={1}{2}", strPlatform, strUniqueDeviceKey, sendinfo);
        }
        else if (sendtype == eSendType.LOGIN_INFO)
        {
            // 게임 로그인시 보낼거
            string strOSInfo = SystemInfo.operatingSystem;
            string strDeviceName = SystemInfo.deviceName;
            string strGamePosition = "mygame"; // CommonUtil.GameEnumToGameName(Data_Mng._Inst.m_eGameName);
            strSendData = string.Format("uuid={0}&os={1}&devicename={2}&facebook={3}&platform={4}&gameposition={5}", strUniqueDeviceKey, strOSInfo, strDeviceName, strFacebookID, strPlatform, strGamePosition);
        }
        else if (sendtype == eSendType.BUY_INFO)
        {
            string strBuyInfo = sendinfo;
            strSendData = string.Format("facebook={0}&buyinfo={1}", strFacebookID, strBuyInfo);
        }

        Debug.Log("JIS strSendData : " + strSendData);

        try
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(strSendData);
            objHttpWebRequest.ContentLength = byteArray.Length;
            objHttpWebRequest.Method = "POST";
            objHttpWebRequest.ContentType = "application/x-www-form-unlencoded";

            objRequestStream = objHttpWebRequest.GetRequestStream();
            objRequestStream.Write(byteArray, 0, byteArray.Length);
            objRequestStream.Close();



            // 응답 받기
            objHttpWebResponse = (HttpWebResponse)objHttpWebRequest.GetResponse();
            if (objHttpWebResponse != null)
            {
                objResponseStream = objHttpWebResponse.GetResponseStream();



                using (StreamReader sr = new StreamReader(objResponseStream))
                {
                    objResponseStream.Flush();
                    string responseString = sr.ReadToEnd();
                    Debug.Log("responseString " + responseString);
                }
                objResponseStream.Close();

                PlayerPrefs.SetInt("eSendType", (int)eSendType.LOGIN_INFO);




                //using (StreamReader sr = new StreamReader(objResponseStream))
                //{
                //    objXMLReader = new XmlTextReader(objResponseStream);
                //    XmlDocument xmldoc = new XmlDocument();
                //    xmldoc.Load(objXMLReader);
                //    XMLResponse = xmldoc;
                //    string response = XMLResponse.InnerText;
                //    Debug.Log("response " + response);
                //    objXMLReader.Close();
                //}


                objResponseStream.Close();
            }

            objHttpWebResponse.Close();
        }
        catch (WebException we)
        {
            Debug.Log(we.Message);
            throw new Exception(we.Message);
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
            throw new Exception(ex.Message);
        }
        finally
        {
            Debug.Log("final!!");
            
            objRequestStream.Close();
            objResponseStream.Close();
            objHttpWebResponse.Close();

            objRequestStream = null;
            objResponseStream = null;
            objHttpWebResponse = null;
            objHttpWebRequest = null;
        }
    }
}
