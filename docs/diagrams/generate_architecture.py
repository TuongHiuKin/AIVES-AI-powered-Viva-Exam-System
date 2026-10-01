"""Generate the concise, editable AIVES architecture diagram."""
from pathlib import Path
from html import escape
OUT = Path(__file__).parent
parts = ['''<svg xmlns="http://www.w3.org/2000/svg" width="1600" height="1060" viewBox="0 0 1600 1060">
<title>AIVES — Kiến trúc hệ thống</title>
<defs><marker id="arrow" markerWidth="10" markerHeight="10" refX="8" refY="5" orient="auto"><path d="M1 1 L9 5 L1 9 Z" fill="#64748b"/></marker></defs>
<rect width="1600" height="1060" fill="#f8fafc"/>
<g font-family="Segoe UI, Arial, sans-serif">''']
def rect(x,y,w,h,fill='#ffffff',stroke='#cbd5e1'):
    parts.append(f'<rect x="{x}" y="{y}" width="{w}" height="{h}" rx="12" fill="{fill}" stroke="{stroke}" stroke-width="2"/>')
def text(x,y,value,size=22,color='#334155',bold=False,anchor='start'):
    parts.append(f'<text x="{x}" y="{y}" font-size="{size}" fill="{color}" font-weight="{700 if bold else 400}" text-anchor="{anchor}">{escape(value)}</text>')
def line(x1,y1,x2,y2,dashed=False):
    parts.append(f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="#64748b" stroke-width="2.4"'+(' stroke-dasharray="8 7"' if dashed else '')+' marker-end="url(#arrow)"/>')
def card(x,y,w,title,subtitle,color):
    rect(x,y,w,100)
    text(x+w/2,y+39,title,24,color,True,'middle')
    text(x+w/2,y+75,subtitle,20,'#64748b',False,'middle')
text(55,65,'AIVES — KIẾN TRÚC HỆ THỐNG',34,'#0f172a',True)
text(55,103,'.NET 8  •  Nhóm 6: Phản hồi & báo cáo',23,'#64748b')

rect(220,155,990,210,'#eaf3ff','#4387cf')
text(715,198,'Presentation Layer (StudentNameMVC)',29,'#245e9c',True,'middle')
card(244,225,300,'Controllers','StudentReports • ClassReports','#245e9c')
card(565,225,300,'Views','Razor • Bootstrap • Chart.js','#245e9c')
card(886,225,300,'ViewModels / DTOs','Dữ liệu hiển thị','#245e9c')
text(105,245,'MVC Web App',22,'#245e9c',True,'middle')
text(105,278,'Startup',20,'#64748b',False,'middle')
line(535,367,535,441)
line(900,443,900,369,True)

rect(220,445,990,245,'#e9f7ef','#45a478')
text(715,488,'Business Layer (AIVES.BLL)',29,'#28734f',True,'middle')
card(244,514,300,'StudentReportService','Báo cáo cá nhân','#28734f')
card(565,514,300,'ClassReportService','Thống kê lớp','#28734f')
card(886,514,300,'Auth / News Services','Nghiệp vụ nền','#28734f')
text(1036,664,'Interfaces • DTOs',21,'#28734f',False,'middle')
text(101,666,'.dll',32,'#334155',True,'middle')
text(101,700,'Class Library',20,'#64748b',False,'middle')
parts.append('<path d="M195 450 Q175 450 175 480 L175 665 Q175 690 160 703 Q175 716 175 741 L175 939 Q175 960 195 960" fill="none" stroke="#94a3b8" stroke-width="2"/>')
line(535,692,535,776)
line(900,778,900,694,True)
text(715,742,'Auth / News',20,'#64748b',False,'middle')

rect(220,780,990,200,'#fff0ed','#d88176')
text(715,822,'Data Access Layer (AIVES.DAL)',29,'#aa5146',True,'middle')
card(244,847,275,'Repositories','Truy cập dữ liệu','#aa5146')
card(566,847,275,'DAOs','Singleton','#aa5146')
card(888,847,298,'AIVESDbContext','EF Core • Entities','#aa5146')
line(524,895,559,895)
line(846,895,881,895)

rect(1245,155,310,162)
text(1268,192,'Chú thích',23,'#334155',True)
line(1268,232,1320,232)
text(1337,239,'Request',22)
line(1268,278,1320,278,True)
text(1337,285,'Response',22)
rect(1245,345,310,154)
text(1268,383,'References',23,'#334155',True)
text(1268,423,'MVC → BLL → DAL',22)
text(1268,464,'MVC → DAL: DI',22)

line(1210,875,1266,875)
line(1266,916,1210,916,True)
parts.append('<path d="M1280 824 L1280 939 C1280 971 1550 971 1550 939 L1550 824 Z" fill="#eef2ff" stroke="#8390b7" stroke-width="2"/>')
parts.append('<ellipse cx="1415" cy="824" rx="135" ry="25" fill="#f4f6ff" stroke="#8390b7" stroke-width="2"/>')
text(1415,891,'SQL Server',28,'#465786',True,'middle')
text(1415,932,'Tài khoản • News',21,'#465786',False,'middle')
text(220,1025,'Kiến trúc tổng quan • Nguồn dữ liệu theo source hiện tại',19,'#64748b')
parts.append('</g></svg>')
(OUT/'AIVES_Architecture.svg').write_text('\n'.join(parts),encoding='utf-8')
