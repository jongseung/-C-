using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
/*
* TCP 서버 - 가위바위보 서버
*기능
*서버에 2개의 클라이언트가 접속
*2개의 클라이언트가 접속하면 게임시작을 알림
*각 클라이언트가 송신한 가위바위보 값을 수신
*가위바위보 결과를 클라이언트에게 송신
*연결된 클라이언트를 종료하고 다음 클라이언트의 접속을 대기
*/
namespace sever
{
    sealed class AllowAllAssemblyVersionsDeserializationBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            String currentAssembly = Assembly.GetExecutingAssembly().FullName;

            // In this case we are always using the current assembly
            assemblyName = currentAssembly;

            // Get the type using the typeName and assemblyName
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                typeName, assemblyName));

            return typeToDeserialize;
        }
    }

    class Program
    {

        static void Main(string[] args)
        {
            //게임에 사용되는 송수신값을 상수로 정의
            //const : 자료형/클래스명 앞에 붙일 수 있음. const로 변수를 만든경우
            //변수에 값을 대입하지 못함. 클래스의 멤버변수의 get만 저장한것과 동일하다.

            const int GAME_START = 1, 
                DISCONNET = -1,
                SCISCCORS = 1,
                ROCK = 2,
                PAPER = 3,
                WIN = 1,
                LOSE = 2,
                DRAW = 3; 
            //변수이름은 대문자로 적는것이 개발자들의 약속이다.
            //절차만 제대로 밟으면 숫자가 겹치더라도 문제가 생기지 않는다.
            // 서버 bind
            TcpListener listener = new TcpListener(9000);
            // 클라이언트 접속 허용
            listener.Start();
            //클라이언트 객체 및 송수신에 사용할 변수 선언
            TcpClient[] clients = { null, null}; //2명 배열
            NetworkStream[] streams = { null, null };
            int[] datas = { 0, 0 }; //2명의 데이터 배열
            BinaryFormatter formatter = new BinaryFormatter();
            int i;
            //무한반복 - 여러 클라이언트 접속 처리 용도 
            for (; ; )
            {
                //2개의 클라이언트 접속 연결
                for (i = 0; i < 2; i++)
                {
                    clients[i] = listener.AcceptTcpClient();
                    streams[i] = clients[i].GetStream();
                }
                //게임 시작 값을 송신
                for(i=0; i<2; i++)
                {
                    formatter.Serialize(streams[i], GAME_START);
                }
                //클라이언트가 보낸 값을 수신
                for (i = 0; i < 2; i++)
                {
                    
                    try
                    {
                        datas[i] = (int)formatter.Deserialize(streams[i]);
                    }
                    catch//클라이언트 데이터를 수신하는 도중 클라이언트가 연결을 끊었을때에 대한 예외처리
                    {
                        Console.WriteLine("{0}번째 클라이언트가 연결을 끊음",i);
                        datas[i] = DISCONNET;
                    }
                }
                //가위바위보 결과 연산
                if (datas[0] == DISCONNET | datas[1] == DISCONNET ) //두 클라이언트 중 한 클라이언트가 연결을 끊은경우의 처리
                {
                    datas[0] = DISCONNET;
                    datas[1] = DISCONNET;
                }
                else if(datas[0] == datas[1])//비겼을때
                {
                    datas[0] = datas[1] = DRAW;
                }
                else if ((datas[0] == SCISCCORS && datas[1] == PAPER )||
                    ( datas[0] == ROCK && datas[1]== SCISCCORS )|| 
                    (datas[0] == PAPER && datas[1]== ROCK))//0번 클라이언트가 이겼을때
                {
                    datas[0] = WIN;
                    datas[1] = LOSE;
                }
                else//1번 클라이언트가 이겼을때
                {
                    datas[0] = LOSE;
                    datas[1] = WIN;
                }

                //결과를 송신 및 클라이언트 연결 끊기 
                for(i=0; i< 2; i++)
                {
                    try
                    {
                        formatter.Serialize(streams[i], datas[i]);
                    }
                    catch
                    {
                        Console.WriteLine("연결이 끊긴 클라이언트");
                    }
                    streams[i].Close();
                    clients[i].Close();
                }
            }
            listener.Stop();

        }
    }
}
