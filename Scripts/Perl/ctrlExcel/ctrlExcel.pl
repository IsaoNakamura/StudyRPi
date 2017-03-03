#!/usr/bin/perl
use Win32::OLE qw(in with);
use Win32::OLE::Const 'Microsoft Excel';

# エクセルオブジェクトを取得(新たにExcelを立ち上げる)
my $excel = Win32::OLE->new('Excel.Application') || die "cannot get active excel!";

# エクセルオブジェクトを取得(既に立ち上がってるExcelから取得する)
# my $excel = Win32::OLE->GetActiveObject('Excel.Application') || die "cannot get active excel!";

# ブックを追加
my $book = $excel->Workbooks->add();

# シートを取得
my $sheet = $book->ActiveSheet;

# セルに値を書き込む
$sheet->Range("A1")->{Value} = "Col1";
$sheet->Range("B1")->{Value} = "Col2";
$sheet->Range("C1")->{Value} = "Col3";

# セルの値を取得する
my $data = $sheet->Range("A1")->{Value};
print $data . "\r\n";

# セルの値を取得する(範囲指定)
my $array = $sheet->Range("A1:C1")->{Value};
print $$array[0][0]  . "\r\n";
print $$array[0][1]  . "\r\n";
print $$array[0][2]  . "\r\n";

# ブックを名前を付けて保存する
$book->SaveAs('C:\Users\isao_nakamura.GEOGIKEN\Desktop\GitHub\StudyRPi\Scripts\Perl\ctrlExcel\test.xlsx');

# ブックを閉じる
$book->Close();

# エクセルを閉じる
$excel->Quit();

