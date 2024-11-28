export class BrowserDataHelper {

    constructor() {

    }

    // Local storage, with expiration custom logic    
    static setLocalStorage(name: string, value: string, expirationDays: number = 0){
        window.localStorage.setItem(name, value);

        // Custom Expiration Logic
        if(expirationDays > 0) {
            var expDate = new Date();
            expDate.setDate(expDate.getDate()+expirationDays);
            window.localStorage.setItem(name+"-expiration", expDate.toUTCString());
        }
    }

    static setLocalStorageTyped<T>(name: string, value: T, isJson: boolean, expirationDays: number = 0) {
        window.localStorage.setItem(name, isJson ? JSON.stringify(value) : value as string);

        // Custom Expiration Logic
        if(expirationDays > 0) {
            var expDate = new Date();
            expDate.setDate(expDate.getDate()+expirationDays);
            window.localStorage.setItem(name+"-expiration", expDate.toUTCString());
        }
    }

    static getLocalStorage(name: string) : string | null {
        var value = window.localStorage.getItem(name);
        if(value) {
            var exp = window.localStorage.getItem(name+"-expiration");
            if(exp){
                var expDate = new Date(Date.parse(exp));
                if(expDate && expDate < new Date()){
                    // Expired
                    window.localStorage.removeItem(name);
                    window.localStorage.removeItem(name+"-expiration");
                    return null;
                }
            }
            return value;
        } 
        return null;
    }

    static getLocalStorageTyped<T>(name: string, isJson: boolean) : T | null {
        var value = window.localStorage.getItem(name);
        if(value) {
            var exp = window.localStorage.getItem(name+"-expiration");
            if(exp){
                var expDate = new Date(Date.parse(exp));
                if(expDate && expDate < new Date()){
                    // Expired
                    window.localStorage.removeItem(name);
                    window.localStorage.removeItem(name+"-expiration");
                    return null;
                }
            }
            return isJson ? JSON.parse(value) as T : value as T ;
        } 
        return null;
    }

    static removeLocalStorage(name: string) {
        window.localStorage.removeItem(name);
    }

}