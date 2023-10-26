NodaTime : Time 에 관한 Duration, Interval 클래스 기능을 제공한다.
 이를 이용하여 데이터의 time 값을 Tick을 기준으로 interval과 duration을 구한다.
 Tick 은 100 ns의 값을 갖는다.  사용되는 센서데이터는 10msec 이며, 각 시간 단위별로 값의 크기는 아래와 같다
 - 10 ms
 - 10,000 us
 - 10,000,000 ns
 - 100,000 Tick

 NLog : console과 각종 log 기록을 하는 package, 실행프로젝트에서는 NLog.config 파일에서 저장할 target 정보를 갖는 파일이 있어야한다.
