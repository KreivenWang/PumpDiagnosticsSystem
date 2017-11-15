[TOC]

# 版本说明

`#20170904_V1`：

1. 判据解析
2. 故障项报警
3. 判据严重度分档

`#20170926_V2`:

1. 加了好多东西, 一些很细的逻辑, 不一一说明了

`#20171101_V2.1`:

1. 添加了复杂频谱的分析（分频段、底脚噪声、边频带）
2. 添加了底脚噪声分档，9档
3. 修复了诊断报告数据库中图表信息不完整的情况

# 概念

## 注意点
- 规定所有的GUID都使用全大写，所有组件包括机泵、传感器等

## `间接属性`
是属于部件本身的属性，所谓间接，是指需要通过变量来绑定到一个信号量，从而获取参数值。  
>例如压力传感器的压力值，直接绑定到一个信号量就可以啦。

## `引用属性`
属于其他部件的属性，所谓引用，是指引用了其他传感器或部件的`间接属性`的值作为自己的值。   
>***例如我要获取电机的温度，可是有3个温度传感器，怎么对应呢？***  
>很简单，`组件库`中有个属性叫`传感器位置`（`Position`），传感器在定义时就会按信号量名称设置自己的位置，引用属性寻找传感器时自然就找到对应位置的传感器，然后就能拿到它的信号量啦。

## 关于判据解析
`PREV`函数需要`实时数据列表`，来保存一定时间内的数据，但用到PREV的判据都不在振动相关的故障内，所以暂时不解析带`PREV`函数的判据

### 举个例子
判据如下（电机驱动端轴承缺损）：
>(`SpectrumIntegration`(`@Spectrum_Bearing_In_Y`,0.8\*`@Speed`,1.2\*`@Speed`,`#SPECTRUMINTERVAL`\*60)\*1.5)<`SpectrumIntegration`(`@Spectrum_Bearing_In_Y`,1.8\*`@Speed`,2.2\*`@Speed`,`#SPECTRUMINTERVAL`*60)

- 1、替换常量（#）  
  设 `#SPECTRUMINTERVAL` = 10
>(`SpectrumIntegration`(`@Spectrum_Bearing_In_Y`,0.8\*`@Speed`,1.2\*`@Speed`,***10***\*60)\*1.5)<`SpectrumIntegration`(`@Spectrum_Bearing_In_Y`,1.8\*`@Speed`,2.2\*`@Speed`,***10****60)

- 2、根据实时数据替换变量（@）为具体的信号量  
  设 `@Speed` = `$Motor_Speed`  
  设 `@Spectrum_Bearing_In_Y` = `$Motor_BIY_Spectrum`  

>(`SpectrumIntegration`( ***$^Motor_BIY_Spectrum***,0.8\* ***$Motor_Speed***,1.2\* ***$Motor_Speed***,10\*60)\*1.5)<`SpectrumIntegration`( ***$^Motor_BIY_Spectrum***,1.8\* ***$Motor_Speed***,2.2\* ***$Motor_Speed***,10*60)

- 3、此时获取采集到的信号量实时数据  
  设 `$Motor_Speed` = 100  
  设 `$Motor_BIY_Spectrum` = 1  *[1, -1, 1, -2, 1, -3]*
>(`SpectrumIntegration`(***1***,0.8\****100***,1.2\****100***,10\*60)\*1.5)<`SpectrumIntegration`(***1***,1.8\****100***,2.2\****100***,10*60)

# 日志中的错误处理建议
- 判据提示【@变量的变量值】无法解析
  - 正常原因：
    - 传感器没有值
  - 错误原因：
    - access中表PHYEF/PHYDEF_NONVIBRA中可能少了信号量的定义，或没有启用该信号量

- 判据提示【@变量】无法解析
  - 错误原因：
    - 判据中的变量写错了
    - 间接参数表中没有定义该变量

- 判据提示【@变量】没有变量值
  - 正常原因：
    - 实际情况下没有该变量值，如：泰和-电机-@Spectrum_Bearing_In_Z没有变量值，那意思就是泰和电机本来就没有轴承轴向的测点。

# 通过的判据的存储方式
- 需要加入报警过滤功能，对连续通过的判据进行报警，少于指定次数（如只发生一次）的不报警，次数应该在判据模板中设定
- 每种故障一天只报警一次，或可以设置一天最多报几次
  - 同一种故障判断：故障模式相同，组件代号相同，则认为是同一种故障

# 定义

## 数据流向和过程
1. 根据泵找到所有传感器, 基本上分振动/非振动/单独的一个转速
2. 构建实时数据`RtData`, 其中会设置信号量与RedisKeyMap的对应关系
3. 读取实时数据, 把`pumpSystem`对象中有绑定的信号量全部替换成实时数据

## 特征频率

### 转速频率

转速频率，又称工频（工作频率）。

有RPM（Revolutions Per Minute），每分钟的转速。

有RPS（Revolutions Per Second），每秒钟的转速。

一般以水泵的转速为准，转速传感器一般安装在联轴器位置，一台机组只有一个转速传感器。

### 轴承缺陷频率

轴承缺陷频率分为4种：

- BPFI：轴承内圈缺陷频率
- BPFO：轴承外圈缺陷频率
- BSF：轴承滚子缺陷频率
- FTF：轴承保持架缺陷频率

### 叶片通过频率

叶片通过频率，又称BPF（Blade Pass Frequency），指水泵的叶片通过频率，数值一般是叶片数倍的工频。

### 固有频率

介质自身的频率。

一般在主峰为固有频率边频带的判断中用到，该情况下判断为固有频率的方法是：主峰频率为非其他任何特征频率。

## 枚举标识

### 轴承位置排序
从左到右依次为:  
>0 - 水泵非驱动端  
>1 - 水泵驱动端  
>2 - 电机驱动端  
>3 - 电机非驱动端  



### 特征值名称(`FtName`)

| FtName | Flag | BinaryValue |  Remark   |
| :----: | :--: | :---------: | :-------: |
|  RPS   |  1   | `0000 0001` |  工频（转/秒）  |
|  BPFI  |  2   | `0000 0010` | 轴承内圈缺陷频率  |
|  BPFO  |  4   | `0000 0100` | 轴承外圈缺陷频率  |
|  BSF   |  8   | `0000 1000` | 轴承滚子缺陷频率  |
|  FTF   |  16  | `0001 0000` | 轴承保持架缺陷频率 |
|  BPF   |  32  | `0010 0000` |  叶片通过频率   |

------



### 主峰边频带(`SidePeakGroupType`)

| Main |  Side   | Flag | BinaryValue |      Remark      |
| :--: | :-----: | :--: | :---------: | :--------------: |
| BPFI | RPS/FTF |  1   | `0000 0001` |   内圈谐波为主峰的边频带    |
| BSF  |   FTF   |  2   | `0000 0010` |   滚子谐波为主峰的边频带    |
|  NF  |  BPFO   |  4   | `0000 0100` |  固有频率谐波为主峰的边频带   |
| BPFO |  BPFO   |  8   | `0000 1000` |   外圈谐波为主峰的边频带    |
| BPF  |   RPS   |  16  | `0001 0000` | 叶轮通过频率的谐波为主峰的边频带 |

------



### 频段划分(`FreqencyRegionType`)

| Region | Flag | BinaryValue |      Remark      |
| :----: | :--: | :---------: | :--------------: |
|  Low   |  1   |   `0001`    |    0 - 40xRPM    |
| Middle |  2   |   `0010`    | 40xRPM - 50%FMax |
|  High  |  4   |   `0100`    |  50%Fmax - FMax  |

------



### 底脚噪声档位划分(`FooterGradeType`)

| Grade  | Flag | BinaryValue |                  Remark                  |
| :----: | :--: | :---------: | :--------------------------------------: |
|  None  |  0   |   `0000`    |                  不需要分档                   |
|  Low   |  1   |   `0001`    |                   低档底脚                   |
| Middle |  2   |   `0010`    |                   中档底脚                   |
|  High  |  4   |   `0100`    |                   高档底脚                   |
|  All   |  8   |   `1000`    | 所有档位在一起的判断, 3档加一起 != All, 因为All代表可以跨越3个档位, 而加一起不可以, 所以不要用7(Low+Middle+High)! |

> *除以上几档之外，不要使用类似1+2、2+4等档位组合，因为目前程序中是按照一档一档来判断的，无法判断跨档的组合。*
>

# 频谱分析

## 频谱图的构成

对于每个位置的振动传感器（水泵/电机|X/Y/Z|驱动端/非驱动端），每一次振动数据采集都会有一个频谱图。

### 分辨率

分辨率=频带宽度/采集到的频率点个数。

现实际情况以1000(Hz)的频带宽度，800个频率点计算，得到1.25(Hz)每个点的分辨率。

### 坐标轴

X轴（单位：Hz）每个点一个分辨率。

现实际情况为0、1.25(Hz)、2.5(Hz)、3.75(Hz)、5(Hz)……以此类推，共1个起始点+800个数据点。

> *注：为了使数据索引与代码中的数组索引简单同步，采用了**第0个点**的方法，坐标为（0,0），对频谱而言没有实际意义*

Y轴（单位：mm/s）表示每个频率上的振动速度。

之后可以加速度值代替。

### 报警值

报警值为振动报警总值的简称，按斯凯孚（SKF）培训材料为准，取了常量11.42为报警值。

## 底脚噪声分档

手动分档体现在判据表(`CriterionToBuild`)中。

自动分档体现在判据的字段`#FOOTER_GRADE_COEFF`(底脚噪声分档系数)中。

`Range`含义：报警值（频谱图上的Y值）的分档范围，单位：Hz。

|    Range    | Definition (Manual) | Definition (Auto) |
| :---------: | :-----------------: | :---------------: |
| $b^0$~$b^1$ |         Low         |         1         |
| $b^1$~$b^2$ |         Low         |         2         |
| $b^2$~$b^3$ |         Low         |         3         |
| $b^3$~$b^4$ |       Middle        |         1         |
| $b^4$~$b^5$ |       Middle        |         2         |
| $b^5$~$b^6$ |       Middle        |         3         |
| $b^6$~$b^7$ |        High         |         1         |
| $b^7$~$b^8$ |        High         |         2         |
| $b^8$~$b^9$ |        High         |         3         |

> *b为底数（目前值为1.6）*

## 边频带与底脚噪声分析
>  判断：
>  对于任意一个`Spectrum`频谱图，在指定`FreqenceyRegions`（频段：低中高频），是否存在`FooterNoise`（底脚噪声：按严重度分9档，3档手动调整（低中高）x3档自动分档）, 且底脚噪声中至少存在`N`个`SidePeakGroup`（主峰边频带）


```flow
st=>start: Start
spec=>inputoutput: 输入频谱图spec
fregion=>inputoutput: 输入频段fregion
fnoise=>inputoutput: 输入底脚噪声fnoise
spg=>inputoutput: 输入主峰边频带spgroup
n1=>inputoutput: 输入主峰边频带个数n

canFindSpec=>condition: 找到频谱图
isNeedNoise=>condition: 需要底脚噪声
canFindNoiseDots=>condition: 找到底脚点集合
canFindSpgroups=>condition: 区域内找到主峰边频带/N倍谐波

filteNoiseDot=>subroutine: 根据频段筛选噪声点区域
filteDot=>subroutine: 根据频段筛选区域
pass=>operation: 判据通过（故障状态）
npass=>operation: 判据不通过（正常状态）
e=>end

st->spec->fregion->fnoise->spg->n1->canFindSpec(no)->npass
canFindSpec(yes)->isNeedNoise(yes)->canFindNoiseDots(yes)->filteNoiseDot->canFindSpgroups(yes)->pass
canFindSpgroups(no)->npass
isNeedNoise(no)->filteDot->canFindSpgroups(yes)->pass
canFindSpgroups(no)->npass
canFindNoiseDots(no)->npass
pass->e
npass->e
```

```mermaid
graph LR
A --> B
```

#诊断报告

## 相同故障的判定策略

以下字段都相同，则认为是同一故障：

- 组件代号（`CompCode`）
- 显示文字（`DisplayText`）
- 判据在库中Id（`CriterionBuiltIds`）
- 严重度（`Severity`）

## 报告中的时间字段

- `FirstTime`：
  - 故障第一次发生的时间
- `LatestTime`：
  - 故障最近一次发生的时间
- `RecordTime`：
  - 记录故障的时间

举个例子：

1. 假设任意故障`A`发生，此时获取`A`的通过的判据列表`Cts`
2. 从`Cts`中找到时间最新的一条判据`Ct`的发生时间`HappenTime` ，分别作为`A`的`FirstTime`和`LatestTime`
3. 根据**相同故障的判定策略**比对历史故障报告，寻找相同故障`B`
4. 若`B`存在，则用`A`的`LatestTime`更新`B`的`LatestTime`；若`B`不存在，则`A`作为新的报告加入。