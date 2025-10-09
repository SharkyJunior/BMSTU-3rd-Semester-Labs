import csv
from statistics import mean
from math import sqrt, floor, ceil

class CSVEngine:
    STEP = 5
    
    def __init__(self):
        self.headers = []
        self.column_types = []
        self.data = []
        self.row_count = -1
        self.regions = {}
        self.region_col_index = -1
        
    def parseCsv(self, filename: str):
        if not filename.endswith('.csv'):
            raise ValueError('\nNot a CSV file!\n')
        try:
            with open(filename, "r") as f:
                csvreader = csv.reader(f, delimiter=',')
                
                row = next(csvreader)
                for val in row:
                    try:
                        float(val)
                        raise ValueError("Invalid header.")
                    except:
                        self.headers.append(val)

                column_count = len(self.headers)
                
                row = next(csvreader)
                self.row_count = 1
                self.data.append([])
                for val in row:
                    try:
                        value = int(val)
                        value_type = int
                    except ValueError:
                        try:
                            value = float(val)
                            value_type = float
                        except ValueError:
                            value = val
                            value_type = str
                    self.data[0].append(value)
                    self.column_types.append(value_type)
                
                if column_count != len(self.data[0]):
                    raise Exception("Invalid CSV.")
                
                for row in csvreader:  
                    
                    if column_count != len(row) or '' in row:
                        continue
                    
                    cur_data = []
                    
                    for i in range(column_count):
                        try:
                            value = int(row[i])
                        except ValueError:
                            try:
                                value = float(row[i])
                            except ValueError:
                                value = row[i]
                        
                        if type(value) != self.column_types[i]:
                            break
                        
                        cur_data.append(value)
                    else:
                        # print(cur_data)
                        self.row_count += 1
                        self.data.append(cur_data)
        except OSError:
            raise OSError('Couldn\'t open the file!')
        except BaseException as e:
            raise e
        
        self.__parseRegions()
        
        print("\nFile read successfully!\n")
    
    def __parseRegions(self):
        self.region_col_index = self.headers.index('region')
        if self.region_col_index == -1:
            raise ValueError('No \'region\' column found.')  
        
        region_list = []
        for row in self.data:  
            if row[self.region_col_index] not in region_list:
                region_list.append(row[self.region_col_index])
        
        region_list.sort()
        for i in range(len(region_list)):
            self.regions[f"{i+1}"] = region_list[i]
    
    def getRegionData(self, region_id: int) -> list[list]:
        if region_id < 1 or region_id > len(self.regions):
            raise ValueError('\nInvalid region selected!\n')
        
        targetRegion = self.regions[str(region_id)]
        regionData = []
        for row in self.data:
            if row[self.region_col_index] == targetRegion:
                regionData.append(row)
        
        return regionData
    
    def getMetricsByColumnId(self, region_id: int, column_id: int) -> dict:
        if 1 > region_id or region_id > len(self.regions):
            raise ValueError('\nInvalid region selected!\n')
        if 1 > column_id or column_id > len(self.headers):
            raise ValueError('\nInvalid column selected!\n')
        if self.column_types[column_id - 1] == str:
            raise TypeError('\nCan\'t calculate metrics for NaN\n')
        
        targetRegion = self.regions[str(region_id)]
        values = []
        for row in self.data:
            if row[self.region_col_index] == targetRegion:
                values.append(row[column_id - 1])
                
        values.sort()
        metrics = {'region': targetRegion,
                   'column': self.headers[column_id - 1],
                   'max': max(values),
                   'min': min(values),
                   'mean': mean(values),
                   'standard_deviation': self.__standardDeviation(values)}
             
        N = len(values)      
        if N % 2 != 0:
            metrics['median'] = values[N // 2]
        else:
            metrics['median'] = (values[floor(N / 2)] + values[ceil(N / 2)]) / 2
        
        percentiles = {}
        for i in range(0, 101, self.STEP):
            if i == 50:
                percentiles[f'{i}'] = metrics['median']
            else:
                percentiles[f'{i}'] = values[max(round((i / 100) * N) - 1, 0)]
            
        metrics["percentiles"] = percentiles
        
        return metrics
        
    def __standardDeviation(self, values: list):
        avg = mean(values)
        return sqrt(sum([(i - avg)**2 for i in values]) / len(values))
        
    
        
        

        
        