"""Merged form cells shared by PDF and Excel; no application-specific values."""
from math import ceil


EDGES = {'l': 'LINEBEFORE', 'r': 'LINEAFTER', 't': 'LINEABOVE', 'b': 'LINEBELOW'}


def pdf(block, available, paragraph, table_type):
    from reportlab.lib import colors
    from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_RIGHT
    from reportlab.platypus import TableStyle

    data = [[''] * len(block['widths']) for _ in block['heights']]
    commands = [('VALIGN', (0, 0), (-1, -1), 'MIDDLE'),
                ('LEFTPADDING', (0, 0), (-1, -1), 3), ('RIGHTPADDING', (0, 0), (-1, -1), 3),
                ('TOPPADDING', (0, 0), (-1, -1), 2), ('BOTTOMPADDING', (0, 0), (-1, -1), 2)]
    if not block.get('cell_edges'):
        commands.append(('GRID', (0, 0), (-1, -1), .4, colors.black))
    for cell in block['cells']:
        column, row = cell['column'], cell['row']
        align = {'l': TA_LEFT, 'r': TA_RIGHT}.get(cell.get('align'), TA_CENTER)
        data[row][column] = paragraph(cell['text'], align, cell.get('size', 8), cell.get('bold', False),
                                      href=cell.get('href'))
        last = (column + cell.get('colspan', 1) - 1, row + cell.get('rowspan', 1) - 1)
        if last != (column, row):
            commands.append(('SPAN', (column, row), last))
        commands.extend((EDGES[edge], (column, row), last, .4, colors.black)
                        for edge in cell.get('edges', ''))
    total = sum(block['widths'])
    table = table_type(data, colWidths=[available * value / total for value in block['widths']],
                       minRowHeights=block['heights'], splitByRow=0, hAlign='CENTER')
    table.setStyle(TableStyle(commands))
    return table


def xlsx(sheet, block, first_row, write, widths):
    from openpyxl.styles import Alignment, Border, Side
    from openpyxl.utils import get_column_letter

    edge = Side(style='thin', color='000000')
    for index, weight in enumerate(block['widths'], 1):
        dimension = sheet.column_dimensions[get_column_letter(index)]
        dimension.width = widths.observe(index, block.get('key_prefix', 'ANTET_BON_') + block['keys'][index - 1],
                                         weight * 15, dimension.width)
    for cell in block['cells']:
        column, row = cell['column'] + 1, first_row + cell['row']
        last_column = column + cell.get('colspan', 1) - 1
        last_row = row + cell.get('rowspan', 1) - 1
        item = write(row, column, cell['text'], cell.get('bold', False), cell.get('size', 8),
                     numeric=cell.get('numeric', False))
        if item.data_type == 'n' and ',' in cell['text']:
            item.number_format = '#,##0.' + '0' * len(cell['text'].rsplit(',', 1)[1])
        align = {'l': 'left', 'r': 'right'}.get(cell.get('align'), 'center')
        item.alignment = Alignment(horizontal=align, vertical='center', wrap_text=True)
        sides = cell.get('edges', 'lrtb')
        item.border = Border(left=edge if 'l' in sides else None, right=edge if 'r' in sides else None,
                             top=edge if 't' in sides else None, bottom=edge if 'b' in sides else None)
        if last_column != column or last_row != row:
            sheet.merge_cells(start_row=row, start_column=column, end_row=last_row, end_column=last_column)
    return first_row + len(block['heights']) + block.get('space_after', 1)


def fit_rows(sheet, block, first_row):
    """Size merged cells after all tables and configured widths are resolved."""
    from openpyxl.utils import get_column_letter

    heights = list(block['heights'])
    for cell in block['cells']:
        column = cell['column'] + 1
        last_column = column + cell.get('colspan', 1) - 1
        characters = max(1, sum(sheet.column_dimensions[get_column_letter(i)].width
                                for i in range(column, last_column + 1)) * 10 / cell.get('size', 8) - 2)
        lines = sum(max(1, ceil(len(line) / characters)) for line in str(cell['text']).split('\n'))
        required = lines * cell.get('size', 8) * 1.4 + 5
        end = cell['row'] + cell.get('rowspan', 1)
        if required > sum(heights[cell['row']:end]):
            heights[end - 1] += required - sum(heights[cell['row']:end])
    for index, height in enumerate(heights, first_row):
        sheet.row_dimensions[index].height = height
