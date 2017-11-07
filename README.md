# 目录
- [目录](#%E7%9B%AE%E5%BD%95)
- [概念](#%E6%A6%82%E5%BF%B5)
    - [注意点](#%E6%B3%A8%E6%84%8F%E7%82%B9)
        - [`间接属性`](#%E9%97%B4%E6%8E%A5%E5%B1%9E%E6%80%A7)
        - [`引用属性`](#%E5%BC%95%E7%94%A8%E5%B1%9E%E6%80%A7)
    - [关于判据解析](#%E5%85%B3%E4%BA%8E%E5%88%A4%E6%8D%AE%E8%A7%A3%E6%9E%90)
        - [举个例子](#%E4%B8%BE%E4%B8%AA%E4%BE%8B%E5%AD%90)
- [日志中的错误处理建议](#%E6%97%A5%E5%BF%97%E4%B8%AD%E7%9A%84%E9%94%99%E8%AF%AF%E5%A4%84%E7%90%86%E5%BB%BA%E8%AE%AE)
- [通过的判据的存储方式](#%E9%80%9A%E8%BF%87%E7%9A%84%E5%88%A4%E6%8D%AE%E7%9A%84%E5%AD%98%E5%82%A8%E6%96%B9%E5%BC%8F)
- [定义](#%E5%AE%9A%E4%B9%89)
    - [数据流向和过程](#%E6%95%B0%E6%8D%AE%E6%B5%81%E5%90%91%E5%92%8C%E8%BF%87%E7%A8%8B)
    - [轴承数组排序](#%E8%BD%B4%E6%89%BF%E6%95%B0%E7%BB%84%E6%8E%92%E5%BA%8F)
    - [特征值名称(`FtName`)数组顺序](#%E7%89%B9%E5%BE%81%E5%80%BC%E5%90%8D%E7%A7%B0ftname%E6%95%B0%E7%BB%84%E9%A1%BA%E5%BA%8F)
    - [边频带峰组(`SidePeakGroupType`)数组顺序](#%E8%BE%B9%E9%A2%91%E5%B8%A6%E5%B3%B0%E7%BB%84sidepeakgrouptype%E6%95%B0%E7%BB%84%E9%A1%BA%E5%BA%8F)
- [版本说明](#%E7%89%88%E6%9C%AC%E8%AF%B4%E6%98%8E)
- [频谱高级分析](#%E9%A2%91%E8%B0%B1%E9%AB%98%E7%BA%A7%E5%88%86%E6%9E%90)
    - [边频带与底脚噪声分析](#%E8%BE%B9%E9%A2%91%E5%B8%A6%E4%B8%8E%E5%BA%95%E8%84%9A%E5%99%AA%E5%A3%B0%E5%88%86%E6%9E%90)
# 概念
## 注意点
- 规定所有的GUID都使用全大写，所有组件包括机泵、传感器等

### `间接属性`
是属于部件本身的属性，所谓间接，是指需要通过变量来绑定到一个信号量，从而获取参数值。  
>例如压力传感器的压力值，直接绑定到一个信号量就可以啦。

### `引用属性`
属于其他部件的属性，所谓引用，是指引用了其他传感器或部件的`间接属性`的值作为自己的值。   
>***例如我要获取电机的温度，可是有3个温度传感器，怎么对应呢？***  
很简单，`组件库`中有个属性叫`传感器位置`（`Position`），传感器在定义时就会按信号量名称设置自己的位置，引用属性寻找传感器时自然就找到对应位置的传感器，然后就能拿到它的信号量啦。

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

## 轴承数组排序
从左到右依次为:  
>0 - 水泵非驱动端  
1 - 水泵驱动端  
2 - 电机驱动端  
3 - 电机非驱动端  

## 特征值名称(`FtName`)数组顺序
| FtName | Flag  | BinaryValue |
| :----: | :---: | :---------: |
| RPS    | 1     | `0000 0001` |
| BPFI   | 2     | `0000 0010` |
| BPFO   | 4     | `0000 0100` |
| BSF    | 8     | `0000 1000` |
| FTF    | 16    | `0001 0000` |
| BPF    | 32    | `0010 0000` |

## 边频带峰组(`SidePeakGroupType`)数组顺序
| Main  | Side    | Flag  | BinaryValue | Remark |
| :---: | :-----: | :---: | :---------: | ------ |
| BPFI  | RPS/FTF | 1     | `0001`      |        |
| BSF   | FTF     | 2     | `0010`      |        |
| NF    | BPFO    | 4     | `0100`      |        |

# 版本说明
`#20170904_V1`：
1. 判据解析
2. 故障项报警
3. 判据严重度分档

`#20170926_V2`:
1. 加了好多东西, 一些很细的逻辑, 不一一说明了


# 频谱高级分析

标签: 振动频谱分析

---
## 边频带与底脚噪声分析
>  判断：
对于任意一个`Spectrum`频谱图，在指定`FreqenceyRegions`（频段：低中高频），是否存在`FooterNoise`（底脚噪声：按严重度分9档，3档手动调整（低中高）x3档自动分档）, 且底脚噪声中至少存在`N`个`SidePeakGroup`（主峰边频带）


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
canFindSpgroups=>condition: 区域内找到主峰边频带

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