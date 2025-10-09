from csvEngine import *



class Application:
    EXIT_OPTION = 5
    
    def __init__(self, csv_engine: CSVEngine):
        self.engine = csv_engine
        self.selected_region_id = -1
    
    
    def __printRegionList(self):
        for key, value in self.engine.regions.items():
            print(key + ". " + value, end = '; ')
        print('\n')
        
    
    def __printMetrics(self, metrics: dict):
        print(f'\n{metrics["column"]} metrics for {metrics["region"]}\n---\n' +
              f'Max: {metrics["max"]}\n' +
              f'Min: {metrics["min"]}\n' +
              f'Mean: {metrics["mean"]}\n' +
              f'Median: {metrics["median"]}\n' +
              f'Standard deviation: {metrics["standard_deviation"]}\n' + 
              f'Percentiles: {metrics["percentiles"]}\n')
    
    def executeOption(self, option: int) -> None:
        try:
            if option == 1:
                filename = input('Input file location: ')
                self.engine.parseCsv(filename)
                
            elif option == 2:
                self.__printRegionList()
                try:
                    region_id = int(input('Select region ID: '))
                except:
                    raise ValueError('\nPlease select a number.\n')
                if region_id < 1 or region_id > len(self.engine.regions):
                    raise ValueError('\nInvalid region selected!\n')
                self.selected_region_id = region_id
                print('\nRegion selected!\n')
                
            elif option == 3:
                regionData = self.engine.getRegionData(self.selected_region_id)
                
                print('\n', self.engine.headers)
                for row in regionData:
                    print(row)
                    
            elif option == 4:
                for i in range(len(self.engine.headers)):
                    print(f"{i+1}. " + self.engine.headers[i], end = '; ')
                selected_column = int(input('\nSelect a column for calculations: '))
                metrics = self.engine.getMetricsByColumnId(self.selected_region_id,
                                                           selected_column)
                
                self.__printMetrics(metrics)
            
            elif option == 5:
                print("\nGoodbye...\n")
            else:
                raise ValueError("\nUnknown option.\n")
        except BaseException as e:
            raise e   


    def showMenu(self) -> None:
        print('Options:\n' 
            + '1. Open CSV file\n'
            + '2. Select region\n'
            + '3. Show region data\n'
            + '4. Calculate metrics\n' 
            + '5. Exit\n')
        
        
    def run(self) -> None:
        option = -1    
        while option != self.EXIT_OPTION:
            self.showMenu()
            
            try:
                option = int(input('Choose an option: '))
                self.executeOption(option)
            except ValueError as e:
                if e.args != ():
                    print(e.args[0])
                else:
                    print('\nPlease select a valid option.\n')
            except BaseException as e:
                if e.args != ():
                    print(e.args[0])
                else:
                    print('\nUnexpected error\n')
            
            
            
            
        
        
    
